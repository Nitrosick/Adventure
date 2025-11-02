using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  private static Unit enemy;
  private static UnitMove enemyMove;
  private static Tile enemyTile;
  private static List<Unit> playerUnits;
  private static float AttackRange => enemy.Equip.GetRange();
  private static readonly float unitPointOffset = 0.75f;

  // Evaluation coefficients
  private static readonly float distanceWeight = 1.0f;
  private static readonly float coverWeight = 0.5f;
  private static readonly float enemyHitWeight = 5.0f;
  private static readonly float threatPenalty = 1.5f;
  private static readonly float allyHitPenalty = 10.0f;

  public static void Init(Unit unit) {
    enemy = unit;
    enemyMove = enemy.GetComponent<UnitMove>();
    enemyTile = enemy.CurrentTile;
    playerUnits = GetPlayerUnits();

    if (enemy == null || enemyMove == null || enemyTile == null || playerUnits.Count == 0) {
      Debug.LogError("Unit AI initialization error");
    }
  }

  public static void EnemyMove() {
    MovePhaseSkills();

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
      case AIBehaviorType.TryPierceHit:
        MoveToBestPiercingPosition();
        break;
      case AIBehaviorType.Retreat:
        MoveAway();
        break;
      case AIBehaviorType.HoldPosition:
        HoldPosition();
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
      if (IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);

      if (path != null) {
        float dist = GetDistance(tile, playerUnits.First().CurrentTile);

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
      if (IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;
      float dist = playerUnits.Min(t => GetDistance(tile, t.CurrentTile));

      if (dist > furthest) {
        safestTile = tile;
        furthest = dist;
      }
    }

    if (safestTile != null) enemyMove.OnMove(safestTile);
  }

  private static bool LineOfSightClear(Vector3 from, Vector3 to) {
    float offset = enemy.Equip.primary.trajectory == ShotTrajectory.Arc
      ? unitPointOffset * 2
      : unitPointOffset;

    Vector3 fixedFrom = from + new Vector3(0, offset, 0);
    Vector3 fixedTo = to + new Vector3(0, offset, 0);
    Vector3 direction = (fixedTo - fixedFrom).normalized;
    GameObject source = enemy.gameObject;
    float dist = Vector3.Distance(fixedFrom, fixedTo);

    Ray ray = new(fixedFrom, direction);
    RaycastHit[] hits = Physics.RaycastAll(ray, dist, ~0, QueryTriggerInteraction.Collide);

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

  private static float EvaluateTileScore(Tile tile, Unit target, int enemiesHit = 1, int alliesHit = 0) {
    float dist = GetDistance(tile, target.CurrentTile);
    int nearbyEnemies = playerUnits.Count(u => Vector2Int.Distance(tile.Coords, u.CurrentTile.Coords) <= 2);
    int coverBonus = tile.type == TileType.Cover ? 1 : 0;

    float score = 0;
    score += dist * distanceWeight;
    score += coverBonus * coverWeight;
    score -= nearbyEnemies * threatPenalty;
    score += target.GetPriority();
    score += enemiesHit * enemyHitWeight;
    score -= alliesHit * allyHitPenalty;

    return score;
  }

  private static void SetAttackTarget(Unit target, Tile from) {
    if (target == null || from == null) return;
    float dist = Pathfinding.GetCost(from, target.CurrentTile, AttackRange);
    if (dist <= AttackRange && LineOfSightClear(from.GetPos(), target.CurrentTile.GetPos())) {
      enemy.Target = target;
    } else {
      enemy.Target = null;
    }
  }

  private static Tile TryMoveToNeighborOf(Unit target) {
    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (IsTrap(neighbor)) continue;
      if (!TileManager.TileIsWalkable(target.CurrentTile, neighbor)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, neighbor, enemy.CurrentMovePoints);

      if (path != null) {
        enemyMove.OnMove(neighbor);
        return neighbor;
      }
    }
    return null;
  }

  private static float GetDistance(Tile from, Tile to) {
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

  private static bool PlayerHasShooters() {
    return playerUnits.Any(u => u.Type == UnitType.Range);
  }

  // Unit behavior
  private static void MoveToClosestEnemy() {
    Unit closest = playerUnits
      .OrderBy(u => GetDistance(enemyTile, u.CurrentTile))
      .First();

    Tile tile = TryMoveToNeighborOf(closest);
    if (tile != null) SetAttackTarget(closest, tile);
    else ComeCloser();
  }

  private static void MoveToPriorityEnemy() {
    foreach (Unit target in playerUnits) {
      Tile tile = TryMoveToNeighborOf(target);
      if (tile != null) {
        SetAttackTarget(target, tile);
        return;
      }
    }
    ComeCloser();
  }

  private static void KeepDistance() {
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      if (IsTrap(tile)) continue;
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
      if (IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      float dist = GetDistance(tile, priorityTarget.CurrentTile);
      if (dist < closestDistance) {
        closest = tile;
        closestDistance = dist;
        SetAttackTarget(priorityTarget, tile);
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
    else MoveAway();
  }

  private static void HoldPosition() {
    foreach (Unit target in playerUnits) {
      float dist = Pathfinding.GetCost(enemyTile, target.CurrentTile, AttackRange);
      if (dist < 2 || dist > AttackRange) continue;
      if (!LineOfSightClear(enemyTile.GetPos(), target.CurrentTile.GetPos())) continue;
      SetAttackTarget(target, enemyTile);

      if (enemy.Target != null) {
        PhaseManager.NextPhase();
        return;
      }
    }

    PhaseManager.NextPhase();
  }

  private static void MoveToBestPiercingPosition() {
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      if (IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in playerUnits) {
        float dist = Pathfinding.GetCost(tile, target.CurrentTile, AttackRange);
        if (dist > AttackRange || dist < 1) continue;
        Vector3 dir = (target.CurrentTile.GetPos() - tile.GetPos()).normalized;
        float maxDist = AttackRange + 0.1f;

        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        int unitLayer = LayerMask.NameToLayer("Unit");
        RaycastHit[] hits = Calculate.HitsOnTrajectory(tile, target.CurrentTile);

        int enemiesHit = 0;
        int alliesHit = 0;
        bool blocked = false;

        foreach (var hit in hits) {
          var go = hit.collider.gameObject;
          if (go.layer == obstacleLayer) {
            blocked = true;
            break;
          }
          if (go.layer == unitLayer && go.TryGetComponent<Unit>(out var u)) {
            if (u.Relation == UnitRelation.Ally) enemiesHit++;
            else alliesHit++;
          }
        }

        if (blocked || enemiesHit == 0) continue;

        float score = EvaluateTileScore(tile, target, enemiesHit, alliesHit);

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

    MoveToClosestEnemy();
  }

  // Active skills
  public static void AttackPhaseSkills() {
    List<Skill> skills = enemy.Equip.GetActiveSkills()
      .Where(s => s.skillPhases.Contains(BattlePhase.Attack))
      .ToList();

    Unit target = enemy.Target;
    bool skip = true;

    if (enemy.SkillCharges > 0) {
      foreach (Skill skill in skills) {
        switch (skill.skillName) {
          case SkillName.ChargedAttack:
            if (
              target != null &&
              (target.CurrentHealth < target.Health.GetMaxHP() ||
              target.Effects.HasAnyEffect(new string[] { "Stun", "Root" }))
            ) {
              skip = false;
              enemy.IsChargedAttack = true;
            }
            break;
          case SkillName.Block:
          case SkillName.Wall:
            if (target == null && PlayerHasShooters()) {
              // FIXME: Проверка на наличие врагов в радиусе
              skip = false;
              enemy.BlockStance(skill.skillName == SkillName.Block ? "e2" : "e7");
            }
            break;
        }
      }
    }

    if (target != null) enemy.OnAttack();
    else if (skip) PhaseManager.NextPhase();
  }

  private static void MovePhaseSkills() {
    // if (enemy.SkillCharges == 0) return;

    // List<Skill> skills = enemy.Equip.GetActiveSkills()
    //   .Where(s => s.skillPhases.Contains(BattlePhase.Movement))
    //   .ToList();
    // if (skills.Count == 0) return;
  }
}
