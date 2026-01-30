using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  private static Unit enemy;
  private static UnitMove enemyMove;
  private static Tile enemyTile;
  private static List<Unit> playerUnits;
  private static float AttackRange => enemy.Equip.GetRange();

  public static void Init(Unit unit) {
    enemy = unit;
    enemyMove = enemy.GetComponent<UnitMove>();
    enemyTile = enemy.CurrentTile;
    playerUnits = BattleAIHeplers.GetPlayerUnits();

    if (enemy == null || enemyMove == null || enemyTile == null || playerUnits.Count == 0) {
      Debug.LogError("Unit AI initialization error");
    }
  }

  public static void EnemyMove() {
    BattleAISkills.MovePhaseSkills(enemy);

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

  private static void ComeCloser() {
    List<Tile> walkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    float bestScore = Mathf.Infinity;

    foreach (Tile tile in walkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit player in playerUnits) {
        float dist = BattleAIHeplers.GetDistance(tile, player.CurrentTile);

        if (dist < bestScore) {
          bestScore = dist;
          bestTile = tile;
        }
      }
    }

    if (bestTile != null) enemyMove.OnMove(bestTile);
    else PhaseManager.NextPhase();
  }

  private static void MoveAway() {
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable(enemyTile);
    Tile safestTile = null;
    float furthest = float.NegativeInfinity;

    foreach (Tile tile in allWalkableTiles) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;
      float dist = playerUnits.Min(t => BattleAIHeplers.GetDistance(tile, t.CurrentTile));

      if (dist > furthest) {
        safestTile = tile;
        furthest = dist;
      }
    }

    if (safestTile != null) enemyMove.OnMove(safestTile);
    else PhaseManager.NextPhase();
  }

  private static Tile TryMoveToNeighborOf(Unit target) {
    if (target.CurrentTile.Neighbors.Contains(enemyTile)) return enemyTile;

    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (BattleAIHeplers.IsTrap(neighbor)) continue;
      if (!TileManager.TileIsWalkable(target.CurrentTile, neighbor)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, neighbor, enemy.CurrentMovePoints);

      if (path != null) {
        enemyMove.OnMove(neighbor);
        return neighbor;
      }
    }
    return null;
  }

  private static void MoveToClosestEnemy() {
    foreach (Unit target in playerUnits.OrderBy(u => BattleAIHeplers.GetDistance(enemyTile, u.CurrentTile))) {
      Tile tile = TryMoveToNeighborOf(target);

      if (tile != null) {
        BattleAIHeplers.SetAttackTarget(enemy, target, tile, AttackRange);
        if (tile == enemyTile) PhaseManager.NextPhase();
        return;
      }
    }
    ComeCloser();
  }

  private static void MoveToPriorityEnemy() {
    foreach (Unit target in playerUnits) {
      Tile tile = TryMoveToNeighborOf(target);

      if (tile != null) {
        BattleAIHeplers.SetAttackTarget(enemy, target, tile, AttackRange);
        if (tile == enemyTile) PhaseManager.NextPhase();
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
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (enemy.Type == UnitType.Range && BattleAIHeplers.HasEnemyTooClose(tile)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in playerUnits) {
        float dist = Pathfinding.GetCost(tile, target.CurrentTile, AttackRange);
        if (dist < 2 || dist > AttackRange) continue;
        if (!BattleAIHeplers.LineOfSightClear(enemy, tile.GetPos(), target.CurrentTile.GetPos())) continue;
        float score = BattleAIHeplers.EvaluateTileScore(playerUnits, tile, target);

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      BattleAIHeplers.SetAttackTarget(enemy, bestTarget, bestTile, AttackRange);
      enemyMove.OnMove(bestTile);
      return;
    }

    Tile closest = null;
    float closestDistance = Mathf.Infinity;
    Unit priorityTarget = playerUnits.First();

    foreach (Tile tile in allWalkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (enemy.Type == UnitType.Range && BattleAIHeplers.HasEnemyTooClose(tile)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      float dist = BattleAIHeplers.GetDistance(tile, priorityTarget.CurrentTile);
      if (dist < closestDistance) {
        closest = tile;
        closestDistance = dist;
        BattleAIHeplers.SetAttackTarget(enemy, priorityTarget, tile, AttackRange);
      }
    }

    if (closest != null) enemyMove.OnMove(closest);
    else PhaseManager.NextPhase();
  }

  private static void HoldPosition() {
    foreach (Unit target in playerUnits) {
      float dist = Pathfinding.GetCost(enemyTile, target.CurrentTile, AttackRange);
      if (dist < 2 || dist > AttackRange) continue;
      if (!BattleAIHeplers.LineOfSightClear(enemy, enemyTile.GetPos(), target.CurrentTile.GetPos())) continue;
      BattleAIHeplers.SetAttackTarget(enemy, target, enemyTile, AttackRange);

      if (enemy.Target != null) {
        PhaseManager.NextPhase();
        return;
      }
    }

    PhaseManager.NextPhase();
  }

  private static void MoveToBestPiercingPosition() {
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    allWalkable.Add(enemyTile);

    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null && tile != enemyTile) continue;

      foreach (Unit target in playerUnits) {
        float dist = Pathfinding.GetCost(tile, target.CurrentTile, AttackRange);
        if (dist > AttackRange) continue;
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

        float score = BattleAIHeplers.EvaluateTileScore(playerUnits, tile, target, enemiesHit, alliesHit);

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      BattleAIHeplers.SetAttackTarget(enemy, bestTarget, bestTile, AttackRange);
      if (bestTile != enemyTile) enemyMove.OnMove(bestTile);
      else PhaseManager.NextPhase();
      return;
    }

    MoveToClosestEnemy();
  }
}
