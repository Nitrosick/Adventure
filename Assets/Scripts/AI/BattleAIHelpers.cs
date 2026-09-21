using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAIHeplers {
  // private static readonly float unitPointOffset = 0.75f;
  // Evaluation weights
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

  public static bool LineOfSightClear(Unit shooter, Unit target) {
    if (shooter == null || target == null) return false;

    ShotTrajectory trajectory = shooter.Equip.primary?.trajectory ?? ShotTrajectory.Direct;

    return trajectory == ShotTrajectory.Arc
      ? ArcLineOfSightClear(shooter, target)
      : DirectLineOfSightClear(shooter, target);
  }

  private static bool DirectLineOfSightClear(Unit shooter, Unit target) {
    Collider shooterCollider = shooter.GetComponent<Collider>();
    Collider targetCollider = target.GetComponent<Collider>();

    if (shooterCollider == null || targetCollider == null) return false;

    Vector3 from = shooterCollider.bounds.center;
    Vector3 to = targetCollider.bounds.center;

    Vector3 direction = (to - from).normalized;
    float distance = Vector3.Distance(from, to);

    RaycastHit[] hits = Physics.RaycastAll(
      from, direction, distance, ~0, QueryTriggerInteraction.Collide
    );

    System.Array.Sort(
      hits, (a, b) => a.distance.CompareTo(b.distance)
    );

    foreach (RaycastHit hit in hits) {
      GameObject hitObject = hit.collider.gameObject;

      if (hitObject == shooter.gameObject) continue;

      if (hit.collider.TryGetComponent(out Unit hitUnit)) {
        if (hitUnit == target) return true;
        if (hitUnit.Relation == shooter.Relation) return false;
        continue;
      }

      int layer = hitObject.layer;

      if (
        layer == LayerMask.NameToLayer("Obstacle") ||
        layer == LayerMask.NameToLayer("BattlefieldTile")
      ) return false;
    }

    return true;
  }

  private static bool ArcLineOfSightClear(Unit shooter, Unit target) {
    Collider shooterCollider = shooter.GetComponent<Collider>();
    Collider targetCollider = target.GetComponent<Collider>();

    if (shooterCollider == null || targetCollider == null) return false;

    Vector3 from = shooterCollider.bounds.center;
    Vector3 to = targetCollider.bounds.center;

    const float arcHeight = 3f;
    const int segments = 12;

    Vector3 previousPoint = from;

    for (int i = 1; i <= segments; i++) {
      float t = i / (float)segments;

      Vector3 currentPoint = Vector3.Lerp(from, to, t);
      currentPoint.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

      Vector3 direction = currentPoint - previousPoint;
      float distance = direction.magnitude;

      RaycastHit[] hits = Physics.RaycastAll(
        previousPoint, direction.normalized, distance, ~0, QueryTriggerInteraction.Collide
      );

      foreach (RaycastHit hit in hits) {
        GameObject hitObject = hit.collider.gameObject;

        if (hitObject == shooter.gameObject) continue;

        if (
          hit.collider.TryGetComponent(out Unit hitUnit) &&
          hitUnit == target
        ) continue;

        int layer = hitObject.layer;

        if (
          layer == LayerMask.NameToLayer("Obstacle") ||
          layer == LayerMask.NameToLayer("BattlefieldTile")
        ) return false;
      }

      previousPoint = currentPoint;
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

    if (unit.Type == UnitType.Range) {
      if (HasEnemyTooClose(fromTile)) return false;
      if (
        !LineOfSightClear(
          unit, target
        )
      ) return false;
    }

    float distance = Pathfinding.GetCost(
      fromTile, target.CurrentTile, attackRange
    );

    if (distance > attackRange) return false;
    return true;
  }

  public static void SetAttackTarget(
    Unit unit,
    Unit target,
    float attackRange
  ) {
    if (
      !target.IsDead &&
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
            unit, player
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
