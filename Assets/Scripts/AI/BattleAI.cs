using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  private static Unit enemy;
  private static UnitMove enemyMove;
  private static Tile enemyTile;
  private static float AttackRange => enemy.Equip.GetRange();

  public static void Init(Unit unit) {
    if (unit == null) {
      Debug.LogError("Unit AI initialization error");
      return;
    }

    enemy = unit;
    enemyMove = enemy.GetComponent<UnitMove>();
    enemyTile = enemy.CurrentTile;

    if (enemyMove == null || enemyTile == null) {
      Debug.LogError("Unit AI initialization error");
    }
  }

  public static List<Unit> PlayerUnits() {
    return QueueManager.Queue
      .Where(unit => unit.Relation == UnitRelation.Ally && !unit.IsDead)
      .OrderByDescending(unit => unit.GetPriority())
      .ToList();
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
    List<Unit> players = PlayerUnits();
    List<Tile> walkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    float bestScore = Mathf.Infinity;

    foreach (Tile tile in walkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit player in players) {
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
    List<Unit> players = PlayerUnits();
    List<Tile> allWalkableTiles = TileManager.GetAllWalkable(enemyTile);
    Tile safestTile = null;
    float furthest = float.NegativeInfinity;

    foreach (Tile tile in allWalkableTiles) {
      if (tile == enemyTile) continue;
      if (BattleAIHeplers.IsTrap(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;
      float dist = players.Min(t => BattleAIHeplers.GetDistance(tile, t.CurrentTile));

      if (dist > furthest) {
        safestTile = tile;
        furthest = dist;
      }
    }

    if (safestTile != null) enemyMove.OnMove(safestTile);
    else PhaseManager.NextPhase();
  }

  private static bool TryFindTileToShootFrom(out Tile moveTile, out Unit target) {
    moveTile = null;
    target = null;

    if (enemy.Type != UnitType.Range) return false;
    List<Unit> players = PlayerUnits();
    if (players.Count == 0) return false;
    List<Tile> walkable = TileManager.GetAllWalkable(enemyTile);

    foreach (Tile tile in walkable) {
      if (tile == enemyTile) continue;
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (BattleAIHeplers.HasEnemyTooClose(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit player in players) {
        float dist = Pathfinding.GetCost(tile, player.CurrentTile, AttackRange);
        if (dist < 2 || dist > AttackRange) continue;
        if (!BattleAIHeplers.LineOfSightClear(enemy, tile.GetPos(), player.CurrentTile.GetPos())) continue;

        moveTile = tile;
        target = player;
        return true;
      }
    }
    return false;
  }

  private static Tile TryMoveToNeighborOf(Unit target) {
    if (target.CurrentTile.Neighbors.Contains(enemyTile)) {
      if (enemy.Type == UnitType.Range && BattleAIHeplers.HasEnemyTooClose(enemyTile)) return null;
      if (!BattleAIHeplers.CanAttackFromHeight(enemy, enemyTile, target)) return null;
      return enemyTile;
    }

    foreach (Tile neighbor in target.CurrentTile.Neighbors) {
      if (!BattleAIHeplers.CanAttackFromHeight(enemy, neighbor, target)) continue;
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
    List<Unit> players = PlayerUnits();
    foreach (Unit target in players.OrderBy(u => BattleAIHeplers.GetDistance(enemyTile, u.CurrentTile))) {
      Tile tile = TryMoveToNeighborOf(target);

      if (tile != null && BattleAIHeplers.CanAttackFromHeight(enemy, tile, target)) {
        BattleAIHeplers.SetAttackTarget(enemy, target, tile, AttackRange);
        if (tile == enemyTile) PhaseManager.NextPhase();
        return;
      }
    }
    if (enemy.Type == UnitType.Range && TryFindTileToShootFrom(out Tile moveTile, out Unit shootTarget)) {
      BattleAIHeplers.SetAttackTarget(enemy, shootTarget, moveTile, AttackRange);
      enemyMove.OnMove(moveTile);
    } else {
      ComeCloser();
    }
  }

  private static void MoveToPriorityEnemy() {
    List<Unit> players = PlayerUnits();
    foreach (Unit target in players) {
      Tile tile = TryMoveToNeighborOf(target);

      if (tile != null && BattleAIHeplers.CanAttackFromHeight(enemy, tile, target)) {
        BattleAIHeplers.SetAttackTarget(enemy, target, tile, AttackRange);
        if (tile == enemyTile) PhaseManager.NextPhase();
        return;
      }
    }
    if (enemy.Type == UnitType.Range && TryFindTileToShootFrom(out Tile moveTile, out Unit shootTarget)) {
      BattleAIHeplers.SetAttackTarget(enemy, shootTarget, moveTile, AttackRange);
      enemyMove.OnMove(moveTile);
    } else {
      ComeCloser();
    }
  }

  private static void KeepDistance() {
    if (enemy.Type == UnitType.Range && !BattleAIHeplers.CanShootFromCurrentTile(enemy, enemyTile)) {
      if (TryFindTileToShootFrom(out Tile moveTile, out Unit shootTarget)) {
        BattleAIHeplers.SetAttackTarget(enemy, shootTarget, moveTile, AttackRange);
        enemyMove.OnMove(moveTile);
      } else {
        PhaseManager.NextPhase();
      }
      return;
    }

    List<Unit> players = PlayerUnits();
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (enemy.Type == UnitType.Range && BattleAIHeplers.HasEnemyTooClose(tile)) continue;

      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in players) {
        if (!BattleAIHeplers.CanAttackFromHeight(enemy, tile, target)) continue;
        float dist = Pathfinding.GetCost(tile, target.CurrentTile, AttackRange);
        if (dist < 2 || dist > AttackRange) continue;
        if (!BattleAIHeplers.LineOfSightClear(enemy, tile.GetPos(), target.CurrentTile.GetPos())) continue;
        float score = BattleAIHeplers.EvaluateTileScore(players, tile, target);

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

    if (players.Count == 0) {
      PhaseManager.NextPhase();
      return;
    }

    Tile closest = null;
    float closestDistance = Mathf.Infinity;
    Unit priorityTarget = players[0];

    foreach (Tile tile in allWalkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (enemy.Type == UnitType.Range && BattleAIHeplers.HasEnemyTooClose(tile)) continue;
      if (!BattleAIHeplers.CanAttackFromHeight(enemy, tile, priorityTarget)) continue;

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
    if (enemy.Type == UnitType.Range && !BattleAIHeplers.CanShootFromCurrentTile(enemy, enemyTile)) {
      if (TryFindTileToShootFrom(out Tile moveTile, out Unit shootTarget)) {
        BattleAIHeplers.SetAttackTarget(enemy, shootTarget, moveTile, AttackRange);
        enemyMove.OnMove(moveTile);
      } else {
        PhaseManager.NextPhase();
      }
      return;
    }

    List<Unit> players = PlayerUnits();

    foreach (Unit target in players) {
      if (!BattleAIHeplers.CanAttackFromHeight(enemy, enemyTile, target)) continue;
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
    List<Unit> players = PlayerUnits();
    List<Tile> allWalkable = TileManager.GetAllWalkable(enemyTile);
    allWalkable.Add(enemyTile);

    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (enemy.Type == UnitType.Range && tile == enemyTile && BattleAIHeplers.HasEnemyTooClose(tile)) continue;
      List<Tile> path = Pathfinding.FindPath(enemyTile, tile, enemy.CurrentMovePoints);
      if (path == null && tile != enemyTile) continue;

      foreach (Unit target in players) {
        if (!BattleAIHeplers.CanAttackFromHeight(enemy, tile, target)) continue;
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

        float score = BattleAIHeplers.EvaluateTileScore(players, tile, target, enemiesHit, alliesHit);

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
