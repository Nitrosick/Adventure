using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour {
  public Vector2Int Coords { get; private set; }
  public List<Tile> Neighbors { get; set; }
  public Unit OccupiedBy { get; set; }

  public GameObject Grid { get; private set; }
  public GameObject AttackGrid { get; private set; }
  public GameObject Highlight { get; private set; }

  public TileType type;
  public TileSpawnType spawnType;
  public BattleReward loot;
  public ParticleSystem lootPickEffect;

  public int height;
  public bool allyFocusPoint;
  public bool enemyFocusPoint;
  public float lootDropChance;

  private void Awake() {
    Coords = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    Neighbors = new();
    Grid = transform.Find("Grid").gameObject;
    AttackGrid = transform.Find("AttackGrid").gameObject;
    Highlight = transform.Find("Highlight").gameObject;

    if (Grid == null || AttackGrid == null || Highlight == null) {
      Debug.LogError("Tile components initialization error");
    }
  }

  private void OnDestroy() {
    Neighbors.Clear();
    OccupiedBy = null;
  }

  private void OnMouseEnter() { Hover(); }
  private void OnMouseExit() { Unhover(); }

  public void Hover() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    if (!Grid.activeSelf && !AttackGrid.activeSelf) return;
    Highlight.SetActive(true);
  }

  public void Unhover() {
    Highlight.SetActive(false);
  }

  public void ShowGrid() {
    Grid.SetActive(true);
  }

  public void ShowAttackGrid() {
    AttackGrid.SetActive(true);
  }

  public void HideGrid() {
    Grid.SetActive(false);
    Highlight.SetActive(false);
    AttackGrid.SetActive(false);
  }

  public void InitNeighbours() {
    Dictionary<Vector2Int, Tile> tiles = TileManager.tiles;

    foreach (var dir in TileManager.allDirections) {
      Vector2Int neighborCoord = Coords + dir;

      if (tiles.ContainsKey(neighborCoord)) {
        Neighbors.Add(tiles[neighborCoord]);
      }
    }
  }

  public void DropLoot() {
    bool success = Utils.RollChance(lootDropChance);
    if (success) {
      GameObject obj = transform.Find("Loot").gameObject;
      if (obj == null || loot == null || lootPickEffect == null) {
        type = TileType.Open;
        return;
      }
      obj.SetActive(true);
      type = TileType.Loot;
    } else {
      type = TileType.Open;
    }
  }

  public void TakeLoot() {
    Transform obj = transform.Find("Loot");
    if (obj == null || loot == null || lootPickEffect == null) return;
    Instantiate(lootPickEffect, obj.position, Quaternion.identity);
    Destroy(obj.gameObject);
    BattleManager.Reward.Add(loot);
    type = TileType.Open;
  }
}
