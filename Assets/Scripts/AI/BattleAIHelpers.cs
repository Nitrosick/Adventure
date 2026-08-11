using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAIHeplers {
  private static readonly float unitPointOffset = 0.75f;
  private static readonly float distanceWeight = 1.0f;
  private static readonly float coverWeight = 0.5f;
  private static readonly float enemyHitWeight = 5.0f;
  private static readonly float threatPenalty = 1.5f;
  private static readonly float allyHitPenalty = 10.0f;

  public static float GetDistance(Tile from, Tile to) {
    return Vector2Int.Distance(from.Coords, to.Coords);
  }

  public static bool IsTrap(Tile tile) {
    if (tile.type == TileType.Trap) {
      Trap trap = tile.GetComponentInChildren<Trap>();
      if (trap == null) return false;
      if (trap.Relation == UnitRelation.Enemy) return true;
    }
    return false;
  }

  public static bool PlayerHasShooters(List<Unit> playerUnits) {
    return playerUnits.Any(u => u.Type == UnitType.Range);
  }

  public static int CountEnemiesInRange(List<Unit> playerUnits, Unit unit, float range) {
    return playerUnits.Count(u =>
      Vector3.Distance(unit.transform.position, u.transform.position) <= range
    );
  }

  public static bool HasEnemyTooClose(Tile tile) {
    return tile.Neighbors.Any(t =>
      t.OccupiedBy != null &&
      t.OccupiedBy.Relation == UnitRelation.Ally
    );
  }

  public static bool LineOfSightClear(Unit unit, Vector3 from, Vector3 to) {
    float offset = unit.Equip.primary?.trajectory == ShotTrajectory.Arc
      ? unitPointOffset * 1.5f
      : unitPointOffset;

    Vector3 fixedFrom = from + new Vector3(0, offset, 0);
    Vector3 fixedTo = to + new Vector3(0, offset, 0);
    Vector3 direction = (fixedTo - fixedFrom).normalized;
    GameObject source = unit.gameObject;
    float dist = Vector3.Distance(fixedFrom, fixedTo);

    Ray ray = new(fixedFrom, direction);
    RaycastHit[] hits = Physics.RaycastAll(ray, dist, ~0, QueryTriggerInteraction.Collide);

    foreach (var hit in hits) {
      GameObject hitObj = hit.collider.gameObject;
      if (hitObj == source) continue;

      if (hitObj.layer == LayerMask.NameToLayer("Obstacle") ||
          hitObj.layer == LayerMask.NameToLayer("BattlefieldTile")) return false;

      if (hitObj.TryGetComponent<Unit>(out var hitUnit)) {
        if (hitUnit.Relation == unit.Relation) return false;
        continue;
      }
    }
    return true;
  }

  // Tiles checking
  public static float EvaluateTileScore(
    List<Unit> playerUnits,
    Tile tile,
    Unit target,
    int enemiesHit = 1,
    int alliesHit = 0
  ) {
    float dist = GetDistance(tile, target.CurrentTile);
    int nearbyEnemies = playerUnits.Count(u => Vector2Int.Distance(tile.Coords, u.CurrentTile.Coords) <= 2);
    int coverBonus = tile.type == TileType.Cover ? 1 : 0;

    float score = 0;
    // TODO: Добавить все условия оценки
    score += dist * distanceWeight;
    score += coverBonus * coverWeight;
    score -= nearbyEnemies * threatPenalty;
    score += target.GetPriority();
    score += enemiesHit * enemyHitWeight;
    score -= alliesHit * allyHitPenalty;

    return score;
  }

  public static List<Tile> GetReachableTiles(Tile from, float movePoints) {
    List<Tile> result = new();

    foreach (Tile tile in TileManager.GetAllWalkable(from)) {
      List<Tile> path =
        Pathfinding.FindPath(
          from, tile, movePoints
        );

      if (path != null) result.Add(tile);
    }

    return result;
  }

  public static bool CanAttackFromTile(
    Unit unit,
    Tile fromTile,
    Unit target,
    float attackRange
  ) {
    if (
      unit == null ||
      fromTile == null ||
      target == null ||
      target.CurrentTile == null
    ) return false;

    if (
      unit.Type != UnitType.Range &&
      fromTile.height != target.CurrentTile.height
    ) return false;

    float distance = Pathfinding.GetCost(
      fromTile, target.CurrentTile, attackRange
    );

    if (distance > attackRange) return false;

    if (
      !LineOfSightClear(
        unit, fromTile.GetPos(), target.CurrentTile.GetPos()
      )
    ) return false;

    return true;
  }

  public static void SetAttackTarget(
    Unit unit,
    Unit target,
    float attackRange
  ) {
    if (
      CanAttackFromTile(
        unit, unit.CurrentTile, target, attackRange
      )
    ) {
      unit.Target = target;
    }
    else {
      unit.Target = null;
    }
  }

  public static bool CanShootFromCurrentTile(Unit unit, Tile tile) {
    if (unit.Type != UnitType.Range) return true;
    return !HasEnemyTooClose(tile);
  }

  public static Tile FindClimbTile(Tile destination) {
    Tile from = BattleAI.enemyTile;
    Tile bestTile = null;
    float bestDistance = Mathf.Infinity;

    foreach (Tile tile in TileManager.tiles.Values) {
      if (tile.type != TileType.Climb) continue;
      if (tile.climbTo == null) continue;

      float currentDiff = Mathf.Abs(from.height - destination.height);
      float climbDiff = Mathf.Abs(tile.climbTo.height - destination.height);

      if (climbDiff >= currentDiff) continue;

      List<Tile> path = Pathfinding.FindPath(
        from, tile, Mathf.Infinity
      );

      if (path == null) continue;

      float distance =
        GetDistance(
          tile.climbTo, destination
        );

      if (distance < bestDistance) {
        bestDistance = distance;
        bestTile = tile;
      }
    }

    return bestTile;
  }

  public static bool TryFindTileToShootFrom(out Tile moveTile, out Unit target) {
    moveTile = null;
    target = null;
    Unit unit = BattleAI.enemy;
    float range = BattleAI.AttackRange;

    if (unit.Type != UnitType.Range) return false;

    List<Unit> players = BattleAI.PlayerUnits();
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in TileManager.tiles.Values) {
      if (tile.OccupiedBy != null) continue;
      if (IsTrap(tile)) continue;
      if (HasEnemyTooClose(tile)) continue;

      foreach (Unit player in players) {
        float dist =
          Pathfinding.GetCost(
            tile, player.CurrentTile, range
          );

        if (dist < 2 || dist > range) continue;

        if (
          !LineOfSightClear(
            unit, tile.GetPos(), player.CurrentTile.GetPos()
          )
        ) continue;

        float safety =
          players.Min(p =>
            GetDistance(
              tile, p.CurrentTile
            )
          );

        if (safety > bestScore) {
          bestScore = safety;
          moveTile = tile;
          target = player;
        }
      }
    }

    return moveTile != null;
  }

  public static Tile GetAttackTile(Unit target) {
    Tile from = BattleAI.enemyTile;
    Unit unit = BattleAI.enemy;
    float range = BattleAI.AttackRange;

    if (
      target.CurrentTile.Neighbors.Contains(from) &&
      CanAttackFromTile(
        unit, from, target, range
      )
    ) return from;

    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (!CanAttackFromTile(
        unit, neighbor, target, range
      )) continue;

      if (IsTrap(neighbor)) continue;

      if (!TileManager.TileIsWalkable(
        target.CurrentTile,
        neighbor
      )) continue;

      return neighbor;
    }

    return null;
  }
}
