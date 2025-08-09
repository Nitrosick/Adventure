using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  private static Unit enemy;
  private static UnitMove enemyMove;
  private static List<Unit> playerUnits;
  private static readonly float distanceWeight = 1.5f;

  // Evaluation coefficients
  private static readonly float coverWeight = 1.0f;
  private static readonly float threatPenalty = 2.0f;
  private static readonly float unitPointOffset = 1.8f;

  public static void EnemyMove(Unit unit) {
    enemy = unit;
    enemyMove = enemy.GetComponent<UnitMove>();
    playerUnits = GetPlayerUnits();

    switch (enemy.BehaviorType) {
      case AIBehaviorType.Aggressive:
        MoveToClosestEnemy();
        break;
      case AIBehaviorType.PriorityTarget:
        MoveToPriorityEnemy();
        break;
      case AIBehaviorType.KeepDistance:
        KeepDistance();
        break;
      case AIBehaviorType.Retreat:
        MoveAway();
        break;
      case AIBehaviorType.Passive:
        PhaseManager.NextPhase();
        break;
    }
  }

  // Support methods
  private static List<Unit> GetPlayerUnits() {
    return QueueManager.Queue
      .Where(unit => unit.Relation == UnitRelation.Ally && !unit.IsDead)
      .OrderByDescending(unit => unit.GetPriority())
      .ToList();
  }

  private static void ComeCloser() {
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable();
    Tile closest = null;
    float closestDistance = Mathf.Infinity;

    foreach (Tile tile in allWalkableTiles) {
      List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, tile, enemy.CurrentMovePoints);

      if (path != null) {
        float dist = Vector2Int.Distance(tile.Coords, playerUnits[0].CurrentTile.Coords);

        if (dist < closestDistance) {
          closest = tile;
          closestDistance = dist;
        }
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
  }

  private static void MoveAway() {
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable();
    Tile safestTile = null;
    float furthest = float.NegativeInfinity;

    foreach (Tile tile in allWalkableTiles) {
      List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;
      float dist = playerUnits.Min(t => Vector2Int.Distance(tile.Coords, t.CurrentTile.Coords));

      if (dist > furthest) {
        safestTile = tile;
        furthest = dist;
      }
    }

    if (safestTile != null) enemyMove.OnMove(safestTile);
  }

  private static float GetAttackDistance(Tile from, Tile to) {
    Vector2Int diff = from.Coords - to.Coords;
    int dx = Mathf.Abs(diff.x);
    int dy = Mathf.Abs(diff.y);
    int range = enemy.Equip.primary.range;

    if (range <= 3) return Mathf.Max(dx, dy);
    return dx + dy == 0 ? 0 : (dx == 0 || dy == 0 ? dx + dy : dx + dy - 0.5f);
  }

  private static bool LineOfSightClear(Vector3 from, Vector3 to, GameObject source) {
    Vector3 fixedFrom = from + new Vector3(0, unitPointOffset, 0);
    Vector3 fixedTo = to + new Vector3(0, unitPointOffset, 0);
    Vector3 direction = (fixedTo - fixedFrom).normalized;
    float distance = Vector3.Distance(from, fixedTo);

    Ray ray = new(fixedFrom, direction);
    RaycastHit[] hits = Physics.RaycastAll(ray, distance, ~0, QueryTriggerInteraction.Collide);

    foreach (var hit in hits) {
      GameObject hitObj = hit.collider.gameObject;
      if (hitObj == source) continue;
      Unit hitUnit = hitObj.GetComponent<Unit>();

      if (hitUnit != null) {
        Unit sourceUnit = source.GetComponent<Unit>();
        if (hitUnit.Relation == sourceUnit.Relation) return false;
        continue;
      }

      if (hitObj.layer == LayerMask.NameToLayer("Obstacle")) return false;
    }

    return true;
  }

  private static float EvaluateTileScore(Tile tile, Unit target) {
    float distanceFromTarget = Vector2Int.Distance(tile.Coords, target.CurrentTile.Coords);
    int nearbyEnemies = playerUnits.Count(u => Vector2Int.Distance(tile.Coords, u.CurrentTile.Coords) <= 2);
    int coverBonus = tile.type == TileType.Cover ? 1 : 0;
    bool hasLOS = LineOfSightClear(tile.transform.position, target.CurrentTile.transform.position, enemy.gameObject);

    float score = 0;
    score += distanceFromTarget * distanceWeight;
    score += coverBonus * coverWeight;
    score -= nearbyEnemies * threatPenalty;

    return score;
  }

  private static void SetAttackTarget(Unit target) {
    if (target == null) return;

    float distance = GetAttackDistance(enemy.CurrentTile, target.CurrentTile);
    int range = enemy.Equip.primary.range;

    if (distance <= range) enemy.Target = target;
    else enemy.Target = null;
  }

  private static bool TryMoveToNeighborOf(Unit target) {
    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (!TileManager.TileIsWalkable(neighbor)) continue;

      List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, neighbor, enemy.CurrentMovePoints);

      if (path != null) {
        enemyMove.OnMove(neighbor);
        return true;
      }
    }

    return false;
  }

  // Unit behavior
  private static void MoveToClosestEnemy() {
    Unit closest = playerUnits
      .OrderBy(u => Vector2Int.Distance(enemy.CurrentTile.Coords, u.CurrentTile.Coords))
      .First();

    if (TryMoveToNeighborOf(closest)) {
      SetAttackTarget(closest);
      return;
    }

    ComeCloser();
  }

  private static void MoveToPriorityEnemy() {
    foreach (Unit target in playerUnits) {
      if (TryMoveToNeighborOf(target)) {
        SetAttackTarget(target);
        return;
      }
    }

    ComeCloser();
  }

  private static void KeepDistance() {
    List<Tile> allWalkable = TileManager.GetAllWalkable();
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in playerUnits) {
        float dist = GetAttackDistance(tile, target.CurrentTile);
        if (dist < 2 || dist > enemy.Equip.primary.range) continue;
        if (!LineOfSightClear(tile.transform.position, target.CurrentTile.transform.position, enemy.gameObject)) continue;
        float score = EvaluateTileScore(tile, target);

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      SetAttackTarget(bestTarget);
      enemyMove.OnMove(bestTile);
      return;
    }

    Tile closest = null;
    float closestDistance = Mathf.Infinity;
    Unit priorityTarget = playerUnits.First();

    foreach (Tile tile in allWalkable) {
      List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      float dist = Vector2Int.Distance(tile.Coords, priorityTarget.CurrentTile.Coords);
      if (dist < closestDistance) {
        closest = tile;
        closestDistance = dist;
        SetAttackTarget(priorityTarget);
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
    else MoveAway();
  }
}
