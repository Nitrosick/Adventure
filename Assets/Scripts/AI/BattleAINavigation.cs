using System.Collections.Generic;
using UnityEngine;

public static class BattleAINavigation {
  public static void MoveToTile(Tile destination) {
    Tile from = BattleAI.enemyTile;
    Unit unit = BattleAI.enemy;
    UnitMove move = BattleAI.enemyMove;

    if (destination == null || from == null || unit == null || move == null) {
      PhaseManager.NextPhase();
      return;
    }

    if (from.height != destination.height) {
      if (
        from.type == TileType.Climb &&
        from.climbTo != null
      ) {
        if (from.climbTo.OccupiedBy == null) move.Climb();
        return;
      }
      else {
        Tile climbTile = BattleAIHeplers.FindClimbTile(destination);
        if (climbTile == null) {
          PhaseManager.NextPhase();
          return;
        }
        destination = climbTile;
      }
    }

    List<Tile> fullPath = Pathfinding.FindPath(
      from, destination, Mathf.Infinity
    );

    if (fullPath == null || fullPath.Count < 2) {
      PhaseManager.NextPhase();
      return;
    }

    float remainingMp = unit.CurrentMovePoints;
    Tile furthestReachable = from;

    for (int i = 1; i < fullPath.Count; i++) {
      float cost = Pathfinding.GetCost(
        fullPath[i - 1], fullPath[i]
      );

      if (remainingMp < cost) break;

      remainingMp -= cost;
      furthestReachable = fullPath[i];
    }

    if (furthestReachable == from) {
      PhaseManager.NextPhase();
      return;
    }
    move.OnMove(furthestReachable);
  }
}