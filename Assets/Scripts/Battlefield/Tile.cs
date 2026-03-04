using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour {
  public Vector2Int Coords { get; private set; }
  public Vector3 InitPosition { get; private set; }
  public List<Tile> Neighbors { get; set; }
  public Unit OccupiedBy { get; set; }

  public GameObject Grid { get; private set; }
  public GameObject AttackGrid { get; private set; }
  public GameObject Highlight { get; private set; }

  public TileType type;
  public Tile climbTo;
  public TileSpawnType spawnType;
  public Reward loot;

  public int height;
  public bool allyFocusPoint;
  public bool enemyFocusPoint;
  public bool reinforcementFocusPoint;
  public float lootDropChance;

  void Awake() {
    Coords = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    Neighbors = new();
    InitPosition = transform.position;

    Transform grid = transform.Find("Grid");
    if (grid != null) Grid = grid.gameObject;
    Transform attackGrid = transform.Find("AttackGrid");
    if (attackGrid != null) AttackGrid = attackGrid.gameObject;
    Transform highlight = transform.Find("Highlight");
    if (highlight != null) Highlight = highlight.gameObject;
  }

  void OnDestroy() {
    Neighbors.Clear();
    OccupiedBy = null;
  }

  private void OnMouseEnter() { Hover(); }
  private void OnMouseExit() { Unhover(); }

  public void Hover() {
    if (
      EventSystem.current.IsPointerOverGameObject() ||
      Grid == null || AttackGrid == null ||
      (!Grid.activeSelf && !AttackGrid.activeSelf)
    ) return;
    Highlight.SetActive(true);
  }

  public void Unhover() {
    if (Highlight == null) return;
    Highlight.SetActive(false);
  }

  public void ShowGrid() {
    if (Grid == null) return;
    Grid.SetActive(true);
  }

  public void ShowAttackGrid() {
    if (AttackGrid == null) return;
    AttackGrid.SetActive(true);
  }

  public void HideGrid() {
    if (Grid == null || Highlight == null || AttackGrid == null) return;
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

  public Vector3 GetPos() {
    return transform.position + new Vector3(0, height - transform.position.y, 0);
  }

  public void DropLoot() {
    bool success = Randomiser.RollChance(lootDropChance);
    if (success) {
      GameObject obj = transform.Find("Loot").gameObject;
      if (obj == null || loot == null || TileManager.Instance.lootPickEffect == null) {
        type = TileType.Open;
        return;
      }
      obj.SetActive(true);
      type = TileType.Loot;
    }
    else {
      type = TileType.Open;
    }
  }

  public void TakeLoot() {
    Transform obj = transform.Find("Loot");
    if (obj == null || loot == null || TileManager.Instance.lootPickEffect == null) return;
    Instantiate(TileManager.Instance.lootPickEffect, obj.position, Quaternion.identity);
    Destroy(obj.gameObject);

    Transform highlight = transform.Find("HighlightEffect");
    if (highlight != null) highlight.gameObject.SetActive(false);

    BattleManager.Reward.Add(loot);

    if (loot.projectiles > 0) {
      foreach (Unit unit in QueueManager.Instance.Queue.Where(u => u.Relation == UnitRelation.Ally)) {
        unit.AddProjectiles(loot.projectiles);
      }
    }

    type = TileType.Open;
    _ = Toast.Show("success", "Loot picked up");
    LogUI.Instance.Add(loot);
    if (transform.TryGetComponent<TooltipTrigger>(out var tooltip)) tooltip.message = "";
  }

  private GameObject SetTrap(Trap hidden) {
    if (hidden == null || hidden.Relation == UnitRelation.Ally) return null;

    Destroy(hidden.gameObject);

    GameObject prefab = BattleManager.Instance.trapRegistry.Get(hidden.Type);
    if (prefab == null) return null;

    GameObject trap = Instantiate(prefab, transform);
    trap.transform.position = GetPos();
    return trap;
  }

  public GameObject UncoverTrap() {
    Trap hiddenTrap = transform.GetComponentInChildren<Trap>();
    return SetTrap(hiddenTrap);
  }

  public void TriggerTrap() {
    GameObject trap = UncoverTrap();
    if (trap == null) return;
    trap.GetComponent<Trap>().Trigger(QueueManager.Instance.CurrentUnit);
    type = TileType.Open;
  }
}
