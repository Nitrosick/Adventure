using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour {
  public static TileManager Instance;
  public ParticleSystem lootPickEffect;

  public static Dictionary<Vector2Int, Tile> tiles = new();
  public static List<TileFiller> fillers = new();
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
    Instance = this;
    tiles.Clear();
    fillers.Clear();

    foreach (Tile tile in FindObjectsOfType<Tile>(true)) {
      tiles[tile.Coords] = tile;
    }

    foreach (TileFiller tile in FindObjectsOfType<TileFiller>(true)) {
      fillers.Add(tile);
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
    fillers.Clear();
  }

  public static bool TileIsWalkable(Tile from, Tile to) {
    if (from.height != to.height) return false;
    return (
      to.type != TileType.Obstacle &&
      to.type != TileType.Tree &&
      to.type != TileType.Breakable &&
      to.OccupiedBy == null
    );
  }

  public static List<Tile> GetAllWalkable(Tile from) {
    return tiles.Values
      .Where(tile => TileIsWalkable(from, tile))
      .ToList();
  }

  public static List<Tile> GetHighTiles() {
    return tiles.Values
      .Where(tile => tile.height > 1)
      .OrderByDescending(tile => tile.height)
      .ToList();
  }

  public static List<TileFiller> GetFillers() {
    return fillers
      .Where(filler => filler.height > 1)
      .ToList();
  }

  public static Tile GetRandomFreeTile(List<Tile> list) {
    List<Tile> freeTiles = list
      .Where(t => t.OccupiedBy == null)
      .ToList();

    if (freeTiles.Count == 0) return null;
    int i = Random.Range(0, freeTiles.Count);
    return freeTiles[i];
  }

  public static Tile GetRandomFreeTile() {
    List<Tile> freeTiles = tiles.Values
      .Where(t => t.OccupiedBy == null && t.type == TileType.Open)
      .ToList();

    if (freeTiles.Count == 0) return null;
    int i = Random.Range(0, freeTiles.Count);
    return freeTiles[i];
  }

  public static List<Tile> GetSpawns(TileSpawnType type) {
    List<Tile> result = new();
    foreach (Tile tile in tiles.Values) {
      if (tile.spawnType == type && tile.type == TileType.Open) result.Add(tile);
    }
    return result;
  }

  public static void ShowReachableTiles(Tile startTile, float mp) {
    if (QueueManager.CurrentUnit.Relation == UnitRelation.Enemy) return;

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
          TileIsWalkable(current, neighbor)
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
      if (unit.Type == UnitType.Melee && unit.CurrentTile.height != tile.height) continue;
      float dist = Pathfinding.GetCost(unit.CurrentTile, tile, unit.Equip.GetRange());
      int minRange = unit.Type == UnitType.Range ? 2 : 0;
      if (dist >= minRange && dist <= unit.Equip.GetRange()) highlightedTiles.Add(tile);
    }

    int targetsCount = 0;
    foreach (Tile tile in highlightedTiles) {
      if (
        (tile.OccupiedBy != null && tile.OccupiedBy.Relation != unit.Relation) ||
        (tile.type == TileType.Breakable && unit.Equip.CanBreakObjects()) ||
        (tile.type == TileType.Tree && unit.Equip.CanChopTrees())
      ) targetsCount++;
      tile.ShowAttackGrid();
    }
    return targetsCount;
  }


  public static void HideGrid() {
    foreach (Tile tile in tiles.Values) tile.HideGrid();
  }
}
