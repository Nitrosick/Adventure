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

  public static bool LineOfSightClear(Unit unit, Vector3 from, Vector3 to) {
    float offset = unit.Equip.primary?.trajectory == ShotTrajectory.Arc
      ? unitPointOffset * 1.75f
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
    // FIXME: Добавить все условия оценки
    score += dist * distanceWeight;
    score += coverBonus * coverWeight;
    score -= nearbyEnemies * threatPenalty;
    score += target.GetPriority();
    score += enemiesHit * enemyHitWeight;
    score -= alliesHit * allyHitPenalty;

    return score;
  }

  public static bool CanAttackFromHeight(Unit unit, Tile fromTile, Unit target) {
    if (target == null || fromTile == null) return false;
    if (unit.Type == UnitType.Range) return true;
    return fromTile.height == target.CurrentTile.height;
  }

  public static void SetAttackTarget(
    Unit unit,
    Unit target,
    Tile from,
    float range
  ) {
    if (target == null || from == null) return;
    if (!CanAttackFromHeight(unit, from, target)) {
      unit.Target = null;
      return;
    }
    float dist = Pathfinding.GetCost(from, target.CurrentTile, range);

    if (dist <= range && LineOfSightClear(unit, from.GetPos(), target.CurrentTile.GetPos())) {
      unit.Target = target;
    } else {
      unit.Target = null;
    }
  }

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

  public static bool CanShootFromCurrentTile(Unit unit, Tile tile) {
    if (unit.Type != UnitType.Range) return true;
    return !HasEnemyTooClose(tile);
  }
}
