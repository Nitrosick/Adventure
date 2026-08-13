using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI {
  public static Unit enemy;
  public static UnitMove enemyMove;
  public static Tile enemyTile;
  public static float AttackRange => enemy.Equip.GetRange();

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
    return QueueManager.Instance.Queue
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
    Unit closestPlayer = PlayerUnits()
      .OrderBy(p =>
        BattleAIHeplers.GetDistance(
          enemyTile, p.CurrentTile
        ))
      .FirstOrDefault();

    if (closestPlayer == null) {
      PhaseManager.NextPhase();
      return;
    }

    BattleAINavigation.MoveToTile(closestPlayer.CurrentTile);
  }

  private static void MoveAway() {
    List<Unit> players = PlayerUnits();
    Tile safestTile = null;
    float furthestDistance = float.NegativeInfinity;

    foreach (Tile tile in TileManager.tiles.Values) {
      if (BattleAIHeplers.IsTrap(tile)) continue;
      if (tile.OccupiedBy != null) continue;

      float distanceToClosestPlayer =
        players.Min(player =>
          BattleAIHeplers.GetDistance(
            tile, player.CurrentTile
          )
        );

      if (distanceToClosestPlayer > furthestDistance) {
        furthestDistance = distanceToClosestPlayer;
        safestTile = tile;
      }
    }

    if (safestTile != null) BattleAINavigation.MoveToTile(safestTile);
    else PhaseManager.NextPhase();
  }

  private static void MoveToEnemy(IEnumerable<Unit> targets) {
    foreach (Unit target in targets) {
      Tile attackTile = BattleAIHeplers.GetAttackTile(target);

      if (attackTile == null) continue;

      BattleAIHeplers.SetAttackTarget(
        enemy, target, AttackRange
      );

      if (attackTile == enemyTile) PhaseManager.NextPhase();
      else BattleAINavigation.MoveToTile(attackTile);
      return;
    }

    if (
      enemy.Type == UnitType.Range &&
      BattleAIHeplers.TryFindTileToShootFrom(
        out Tile shootTile, out Unit shootTarget
      )
    ) {
      BattleAIHeplers.SetAttackTarget(
        enemy, shootTarget, AttackRange
      );

      BattleAINavigation.MoveToTile(shootTile);
      return;
    }

    ComeCloser();
  }

  private static void MoveToClosestEnemy() {
    MoveToEnemy(
      PlayerUnits().OrderBy(
        p => BattleAIHeplers.GetDistance(
          enemyTile, p.CurrentTile
        )
      )
    );
  }

  private static void MoveToPriorityEnemy() {
    MoveToEnemy(PlayerUnits());
  }

  private static void KeepDistance() {
    List<Unit> players = PlayerUnits();

    List<Tile> reachableTiles =
      BattleAIHeplers.GetReachableTiles(
        enemyTile,
        enemy.CurrentMovePoints
      );

    reachableTiles.Add(enemyTile);

    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in reachableTiles) {
      if (BattleAIHeplers.IsTrap(tile)) continue;

      if (
        enemy.Type == UnitType.Range &&
        BattleAIHeplers.HasEnemyTooClose(tile)
      ) continue;

      foreach (Unit target in players) {
        if (
          !BattleAIHeplers.CanAttackFromTile(
            enemy, tile, target, AttackRange
          )
        ) continue;

        float distance = Pathfinding.GetCost(
          tile,
          target.CurrentTile,
          AttackRange
        );

        if (distance < 2) continue;

        float score = BattleAIHeplers.EvaluateTileScore(
          players, tile, target
        );

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      BattleAIHeplers.SetAttackTarget(
        enemy, bestTarget, AttackRange
      );

      if (bestTile == enemyTile) PhaseManager.NextPhase();
      else BattleAINavigation.MoveToTile(bestTile);

      return;
    }

    Unit closestTarget = players
      .OrderBy(u => BattleAIHeplers.GetDistance(
        enemyTile, u.CurrentTile
      ))
      .First();

    BattleAINavigation.MoveToTile(closestTarget.CurrentTile);
  }

  private static void HoldPosition() {
    foreach (Unit target in PlayerUnits()) {
      if (
        !BattleAIHeplers.CanAttackFromTile(
          enemy, enemyTile, target, AttackRange
        )
      ) continue;

      BattleAIHeplers.SetAttackTarget(
        enemy, target, AttackRange
      );

      PhaseManager.NextPhase();
      return;
    }

    PhaseManager.NextPhase();
  }

  private static void MoveToBestPiercingPosition() {
    List<Unit> players = PlayerUnits();
    Tile bestTile = null;
    Unit bestTarget = null;
    float bestScore = float.NegativeInfinity;

    foreach (Tile tile in TileManager.tiles.Values) {
      if (tile.OccupiedBy != null && tile != enemyTile) continue;
      if (BattleAIHeplers.IsTrap(tile)) continue;

      if (
        enemy.Type == UnitType.Range &&
        tile == enemyTile &&
        BattleAIHeplers.HasEnemyTooClose(tile)
      ) continue;

      foreach (Unit target in players) {
        if (
          !BattleAIHeplers.CanAttackFromTile(
            enemy, tile, target, AttackRange
          )
        ) continue;

        RaycastHit[] hits =
          Calculate.HitsOnTrajectory(
            tile, target.CurrentTile
          );

        int enemiesHit = 0;
        int alliesHit = 0;
        bool blocked = false;

        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        int unitLayer = LayerMask.NameToLayer("Unit");

        foreach (var hit in hits) {
          GameObject go = hit.collider.gameObject;

          if (go.layer == obstacleLayer) {
            blocked = true;
            break;
          }

          if (
            go.layer == unitLayer &&
            go.TryGetComponent<Unit>(out var unit)
          ) {
            if (unit.Relation == UnitRelation.Ally) enemiesHit++;
            else alliesHit++;
          }
        }

        if (blocked || enemiesHit == 0) continue;

        float score =
          BattleAIHeplers.EvaluateTileScore(
            players, tile, target, enemiesHit, alliesHit
          );

        if (score > bestScore) {
          bestScore = score;
          bestTile = tile;
          bestTarget = target;
        }
      }
    }

    if (bestTile != null) {
      BattleAIHeplers.SetAttackTarget(
        enemy, bestTarget, AttackRange
      );

      if (bestTile == enemyTile) PhaseManager.NextPhase();
      else BattleAINavigation.MoveToTile(bestTile);
      return;
    }

    MoveToClosestEnemy();
  }
}
