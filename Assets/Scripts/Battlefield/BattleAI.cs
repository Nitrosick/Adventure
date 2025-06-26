using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAI
{
  private static readonly float unitPointOffset = 1.2f;

  public static void EnemyMove(Unit enemy) {
    if (enemy.Type == UnitType.Melee) EnemyMoveMelee(enemy);
    else if (enemy.Type == UnitType.Range) EnemyMoveRange(enemy);
  }

  private static List<Unit> GetPlayerUnits() {
    return QueueManager.Queue
      .Where(unit => unit.Relation == UnitRelation.Ally && !unit.IsDead)
      .OrderByDescending(unit => unit.GetPriority())
      .ToList();
  }

  private static float GetAttackDistance(Tile from, Tile to) {
    Vector2Int diff = from.Coords - to.Coords;
    int dx = Mathf.Abs(diff.x);
    int dy = Mathf.Abs(diff.y);
    return dx + dy == 0 ? 0 : (dx == 0 || dy == 0 ? dx + dy : dx + dy - 0.5f);
  }

  private static bool ComeCloser(Unit enemy, List<Unit> playerUnits) {
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

    if (closest != null) {
      enemy.GetComponent<UnitMove>().OnMove(closest);
      return true;
    }
    return false;
  }

  private static bool LineOfSightClear(Vector3 from, Vector3 to, GameObject source) {
    Vector3 fixedFrom = from + new Vector3(0, unitPointOffset, 0);
    Vector3 fixedTo = to + new Vector3(0, unitPointOffset, 0);
    Vector3 direction = (fixedTo - fixedFrom).normalized;
    float distance = Vector3.Distance(from, fixedTo);

    Ray ray = new (fixedFrom, direction);
    RaycastHit[] hits = Physics.RaycastAll(ray, distance);

    foreach (var hit in hits) {
      GameObject hitObj = hit.collider.gameObject;
      if (hitObj == source) continue;
      Unit hitUnit = hitObj.GetComponent<Unit>();
      if (hitUnit != null) {
        if (hitUnit.Relation == UnitRelation.Emeny) return false;
        else continue;
      }
      return false;
    }

    return true;
  }

  private static bool MoveAway(Unit enemy, List<Unit> playerUnits) {
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

    if (safestTile != null) {
      enemy.GetComponent<UnitMove>().OnMove(safestTile);
      return true;
    }
    return false;
  }

  private static void EnemyMoveMelee(Unit enemy) {
    List<Unit> playerUnits = GetPlayerUnits();
    List<(Unit target, Tile moveTile)> reachableTargets = new();

    foreach (Unit unit in playerUnits) {
      foreach (Tile neighbor in unit.CurrentTile.Neighbors) {
        if (!TileManager.TileIsWalkable(neighbor) && enemy.CurrentTile != neighbor) continue;

        List<Tile> path = Pathfinding.FindPath(enemy.CurrentTile, neighbor, enemy.CurrentMovePoints);

        if (path != null) {
          reachableTargets.Add((unit, neighbor));
          break;
        }
      }
    }

    reachableTargets = reachableTargets
      .OrderByDescending(pair => pair.target.GetPriority())
      .ToList();

    if (reachableTargets.Count > 0) {
      enemy.Target = reachableTargets[0].target;
      enemy.GetComponent<UnitMove>().OnMove(reachableTargets[0].moveTile);
      return;
    }

    if (!ComeCloser(enemy, playerUnits)) PhaseManager.NextPhase();
  }

  private static void EnemyMoveRange(Unit enemy) {
    List<Unit> playerUnits = GetPlayerUnits();
    List<Tile> allWalkable = TileManager.GetAllWalkable();
    Tile bestTile = null;
    float maxDist = float.NegativeInfinity;

    foreach (Tile tile in allWalkable) {
      var path = Pathfinding.FindPath(enemy.CurrentTile, tile, enemy.CurrentMovePoints);
      if (path == null) continue;

      foreach (Unit target in playerUnits) {
        float dist = GetAttackDistance(tile, target.CurrentTile);

        if (dist >= 2 && dist <= enemy.Equip.primaryWeapon.range) {
          if (!LineOfSightClear(
            tile.transform.position,
            target.CurrentTile.transform.position,
            enemy.gameObject
          )) continue;

          float nearestEnemyDist = playerUnits
            .Min(u => Vector2Int.Distance(tile.Coords, u.CurrentTile.Coords));

          if (nearestEnemyDist > maxDist) {
            bestTile = tile;
            maxDist = nearestEnemyDist;
            enemy.Target = target;
          }
        }
      }
    }

    if (bestTile != null) {
      enemy.GetComponent<UnitMove>().OnMove(bestTile);
      return;
    }

    if (enemy.CurrentHealth >= enemy.TotalHealth / 2) {
      if (!ComeCloser(enemy, playerUnits)) PhaseManager.NextPhase();
    } else {
      if (!MoveAway(enemy, playerUnits)) PhaseManager.NextPhase();
    }
  }
}
