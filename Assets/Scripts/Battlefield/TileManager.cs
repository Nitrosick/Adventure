using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
  public static Dictionary<Vector2Int, Tile> tiles = new();
  public static Tile allyFocusTile;
  public static Tile enemyFocusTile;

  public static readonly Vector2Int[] allDirections = new Vector2Int[] {
    new (0,  1),
    new (1,  0),
    new (0, -1),
    new (-1,  0),
    new ( 1,  1),
    new ( 1, -1),
    new (-1, -1),
    new (-1,  1)
  };

  private void Awake() {
    tiles.Clear();

    foreach (Tile tile in FindObjectsOfType<Tile>()) {
      tiles[tile.Coords] = tile;
    }

    if (tiles.Count < 1) {
      Debug.LogError("Tiles not found");
      return;
    }

    foreach (Tile tile in tiles.Values) {
      if (tile.allyFocusPoint) allyFocusTile = tile;
      else if (tile.enemyFocusPoint) enemyFocusTile = tile;
      tile.InitNeighbours();
    }
  }

  private void OnDestroy() {
    tiles.Clear();
  }

  public static bool TileIsWalkable(Tile tile) {
    return (
      tile.type != TileType.Obstacle &&
      tile.type != TileType.Tree &&
      tile.type != TileType.Breakable &&
      tile.OccupiedBy == null
    );
  }

  public static List<Tile> GetAllWalkable() {
    return tiles.Values
      .Where(tile => TileIsWalkable(tile))
      .ToList();
  }

  public static Tile GetRandomFreeTile(List<Tile> list) {
    List<Tile> freeTiles = new ();

    foreach (Tile tile in list) {
      if (tile.OccupiedBy == null) freeTiles.Add(tile);
    }

    if (freeTiles.Count == 0) return null;
    int i = Random.Range(0, freeTiles.Count);
    return freeTiles[i];
  }

  public static List<Tile> GetSpawns(TileSpawnType type) {
    List<Tile> result = new ();
    foreach (Tile tile in tiles.Values) {
      if (tile.spawnType == type && tile.type == TileType.Open) result.Add(tile);
    }
    return result;
  }

  public static void ShowReachableTiles(Tile startTile, float mp) {
    if (QueueManager.CurrentUnit.Relation == UnitRelation.Emeny) return;

    HideGrid();

    Queue<Tile> frontier = new ();
    Dictionary<Tile, float> costSoFar = new ();

    frontier.Enqueue(startTile);
    costSoFar[startTile] = 0;

    while (frontier.Count > 0) {
      Tile current = frontier.Dequeue();

      foreach (Tile neighbor in current.Neighbors) {
        float newCost = costSoFar[current] + Vector2Int.Distance(current.Coords, neighbor.Coords);

        if (
          newCost <= mp &&
          (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) &&
          TileIsWalkable(neighbor)
        ) {
          costSoFar[neighbor] = newCost;
          frontier.Enqueue(neighbor);
          neighbor.ShowGrid();
        }
      }
    }
  }

  public static int ShowAttackGrid(Unit unit) {
    List<Tile> highlightedTiles = new();

    foreach (Tile tile in tiles.Values) {
      if (tile == unit.CurrentTile) continue;
      float dist = Pathfinding.GetCost(unit.CurrentTile, tile);
      int minRange = unit.Type == UnitType.Range ? 2 : 0;
      if (dist >= minRange && dist <= unit.Equip.primaryWeapon.range + 0.5f) highlightedTiles.Add(tile);
    }

    int targetsCount = 0;
    foreach (Tile tile in highlightedTiles) {
      if (
        (tile.OccupiedBy != null && tile.OccupiedBy.Relation != unit.Relation) ||
        (tile.type == TileType.Breakable && unit.Equip.CanBreakObjects())
      ) targetsCount++;
      tile.ShowAttackGrid();
    }
    return targetsCount;
  }


  public static void HideGrid() {
    foreach (Tile tile in tiles.Values) tile.HideGrid();
  }
}
