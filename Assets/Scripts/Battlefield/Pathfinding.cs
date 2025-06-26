using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
  public static List<Tile> FindPath(Tile start, Tile goal, float mp) {
    if (start == null || goal == null) {
      Debug.LogError("Start or target tile doesn't specified");
      return null;
    }

    List<Tile> openSet = new () { start };
    HashSet<Tile> closedSet = new ();

    Dictionary<Tile, Tile> cameFrom = new();
    Dictionary<Tile, float> gScore = new() { [start] = 0 };
    Dictionary<Tile, float> fScore = new() { [start] = Heuristic(start, goal) };

    while (openSet.Count > 0) {
      Tile current = GetLowestFScore(openSet, fScore);

      if (current == goal) {
        float totalSteps = gScore[current];
        if (totalSteps <= mp) return ReconstructPath(cameFrom, current);
        else return null;
      }

      openSet.Remove(current);
      closedSet.Add(current);

      foreach (Tile neighbor in current.Neighbors) {
        if (!TileManager.TileIsWalkable(neighbor) || closedSet.Contains(neighbor)) continue;

        float tentativeGScore = gScore[current] + Vector2Int.Distance(current.Coords, neighbor.Coords);

        if (tentativeGScore > mp) continue;

        if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
        else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, Mathf.Infinity)) continue;

        cameFrom[neighbor] = current;
        gScore[neighbor] = tentativeGScore;
        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
      }
    }

    return null;
  }

  public static float GetCost(Tile from, Tile to) {
    int dx = Mathf.Abs(from.Coords.x - to.Coords.x);
    int dy = Mathf.Abs(from.Coords.y - to.Coords.y);

    int diag = Mathf.Min(dx, dy);
    int straight = Mathf.Abs(dx - dy);

    return diag * 1.5f + straight;
  }

  private static float Heuristic(Tile a, Tile b) {
    return Vector2Int.Distance(a.Coords, b.Coords);
  }

  private static Tile GetLowestFScore(List<Tile> openSet, Dictionary<Tile, float> fScore) {
    Tile best = openSet[0];
    float bestScore = fScore[best];

    for (int i = 1; i < openSet.Count; i++) {
      float score = fScore[openSet[i]];

      if (score < bestScore) {
        best = openSet[i];
        bestScore = score;
      }
    }

    return best;
  }

  private static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current) {
    var totalPath = new List<Tile> { current };

    while (cameFrom.ContainsKey(current)) {
      current = cameFrom[current];
      totalPath.Insert(0, current);
    }

    return totalPath;
  }
}
