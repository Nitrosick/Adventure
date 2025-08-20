using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  private static Unit enemy;
  private static UnitMove enemyMove;
  private static Tile enemyTile;
  private static List<Unit> playerUnits;
  private static int AttackRange => enemy.Equip.GetRange();
  private static readonly float unitPointOffset = 0.75f;

  // Evaluation coefficients
  private static readonly float distanceWeight = 1.5f;
  private static readonly float coverWeight = 1.0f;
  private static readonly float threatPenalty = 2.0f;

  public static void EnemyMove(Unit unit) {
    enemy = unit;
    enemyMove = enemy.GetComponent<UnitMove>();
    enemyTile = enemy.CurrentTile;
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
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable(enemyTile);
    Tile closest = null;
    float closestDistance = Mathf.Infinity;

    foreach (Tile tile in allWalkableTiles) {
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);

      if (path != null) {
        float dist = Vector2Int.Distance(tile.Coords, playerUnits.First().CurrentTile.Coords);

        if (dist < closestDistance) {
          closest = tile;
          closestDistance = dist;
        }
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
  }

  private static void MoveAway() {
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable(enemyTile);
    Tile safestTile = null;
    float furthest = float.NegativeInfinity;

    foreach (Tile tile in allWalkableTiles) {
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;
      float dist = playerUnits.Min(t => Vector2Int.Distance(tile.Coords, t.CurrentTile.Coords));

      if (dist > furthest) {
        safestTile = tile;
        furthest = dist;
      }
    }

    if (safestTile != null) enemyMove.OnMove(safestTile);
  }

  private static bool LineOfSightClear(Vector3 from, Vector3 to) {
    Vector3 fixedFrom = from + new Vector3(0, unitPointOffset, 0);
    Vector3 fixedTo = to + new Vector3(0, unitPointOffset, 0);
    Vector3 direction = (fixedTo - fixedFrom).normalized;
    GameObject source = enemy.gameObject;
    float distance = Vector3.Distance(fixedFrom, fixedTo);

    Ray ray = new(fixedFrom, direction);
    RaycastHit[] hits = Physics.RaycastAll(ray, distance, ~0, QueryTriggerInteraction.Collide);

    foreach (var hit in hits) {
      GameObject hitObj = hit.collider.gameObject;
      if (hitObj == source) continue;

      if (hitObj.layer == LayerMask.NameToLayer("Obstacle") ||
          hitObj.layer == LayerMask.NameToLayer("BattlefieldTile")) return false;

      if (hitObj.TryGetComponent<Unit>(out var hitUnit)) {
        if (hitUnit.Relation == enemy.Relation) return false;
        continue;
      }
    }
    return true;
  }

  private static float EvaluateTileScore(Tile tile, Unit target) {
    float distanceFromTarget = Vector2Int.Distance(tile.Coords, target.CurrentTile.Coords);
    int nearbyEnemies = playerUnits.Count(u => Vector2Int.Distance(tile.Coords, u.CurrentTile.Coords) <= 2);
    int coverBonus = tile.type == TileType.Cover ? 1 : 0;

    float score = 0;
    score += distanceFromTarget * distanceWeight;
    score += coverBonus * coverWeight;
    score -= nearbyEnemies * threatPenalty;

    return score;
  }

  private static void SetAttackTarget(Unit target, Tile from) {
    if (target == null || from == null) return;
    float distance = Pathfinding.GetCost(from, target.CurrentTile, AttackRange);
    if (distance <= AttackRange && LineOfSightClear(from.GetPos(), target.CurrentTile.GetPos())) {
      enemy.Target = target;
    } else {
      enemy.Target = null;
    }
  }

  private static Tile TryMoveToNeighborOf(Unit target) {
    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (!TileManager.TileIsWalkable(target.CurrentTile, neighbor)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, neighbor, enemy.CurrentMovePoints);

      if (path != null) {
        enemyMove.OnMove(neighbor);
        return neighbor;
      }
    }
    return null;
  }

  // Unit behavior
  private static void MoveToClosestEnemy() {
    Unit closest = playerUnits
      .OrderBy(u => Vector2Int.Distance(enemyTile.Coords, u.CurrentTile.Coords))
      .First();

    Tile tile = TryMoveToNeighborOf(closest);
    if (tile != null) SetAttackTarget(closest, tile);
    else ComeCloser();
  }

  private static void MoveToPriorityEnemy() {
    foreach (Unit target in playerUnits) {
      Tile tile = TryMoveToNeighborOf(target);
      if (tile != null) SetAttackTarget(target, tile);
      else ComeCloser();
    }
    ComeCloser();
  }

  private static void KeepDistance() {
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in playerUnits) {
        float dist = Pathfinding.GetCost(tile, target.CurrentTile, AttackRange);
        if (dist < 2 || dist > AttackRange) continue;
        if (!LineOfSightClear(tile.GetPos(), target.CurrentTile.GetPos())) continue;
        float score = EvaluateTileScore(tile, target);

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      SetAttackTarget(bestTarget, bestTile);
      enemyMove.OnMove(bestTile);
      return;
    }

    Tile closest = null;
    float closestDistance = Mathf.Infinity;
    Unit priorityTarget = playerUnits.First();

    foreach (Tile tile in allWalkable) {
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      float dist = Vector2Int.Distance(tile.Coords, priorityTarget.CurrentTile.Coords);
      if (dist < closestDistance) {
        closest = tile;
        closestDistance = dist;
        SetAttackTarget(priorityTarget, tile);
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
    else MoveAway();
  }
}
