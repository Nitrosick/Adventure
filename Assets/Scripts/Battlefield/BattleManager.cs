using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour {
  public static BattleManager Instance;
  // FIXME: Будут разные префабы ловушек
  public GameObject trapPrefab;
  public GameObject hiddenTrapPrefab;

  private UnitData[] allies;
  private UnitData[] enemies;

  private static List<Tile> allySpawns;
  private static List<Tile> allyShooterSpawns;
  private static List<Tile> enemySpawns;
  private static List<Tile> enemyShooterSpawns;
  private static List<Tile> bossSpawns;

  public static BattleResult? battleResult;
  public static Reward Reward { get; private set; }
  public GameObject corpsePrefab;

  private void Awake() {
    Instance = this;
    battleResult = null;
    Reward = new Reward();

    allies = StateManager.playerUnits
      .Where(u => u.inSquad)
      .OrderByDescending(u => u.type == UnitType.Range)
      .ToArray();

    enemies = StateManager.enemies
      .OrderByDescending(u => u.type == UnitType.Range)
      .ToArray();

    if (allies == null || allies.Length == 0) {
      Debug.LogError("Ally units not found");
      return;
    }
    if (enemies == null || enemies.Length == 0) {
      Debug.LogError("Enemy units not found");
      return;
    }
    if (corpsePrefab == null) {
      Debug.LogError("BattleManager components initialization error");
      return;
    }

    InitSpawnZones();
    SpawnUnits(allies, allySpawns, UnitRelation.Ally);
    SpawnUnits(enemies, enemySpawns, UnitRelation.Enemy);
    SpawnTraps(StateManager.trapsCount);
    InitSupports();
  }

  private void Start() {
    QueueManager.Init();
  }

  private void OnDestroy() {
    allies = null;
    enemies = null;
    allySpawns.Clear();
    enemySpawns.Clear();
    battleResult = null;
    Reward = null;
  }

  private void InitSpawnZones() {
    allySpawns = TileManager.GetSpawns(TileSpawnType.AnyAlly);
    enemySpawns = TileManager.GetSpawns(TileSpawnType.AnyEnemy);
    allyShooterSpawns = TileManager.GetSpawns(TileSpawnType.AllyShooter);
    enemyShooterSpawns = TileManager.GetSpawns(TileSpawnType.EnemyShooter);
    bossSpawns = TileManager.GetSpawns(TileSpawnType.Boss);
  }

  private void SpawnUnits(UnitData[] unitsData, List<Tile> spawns, UnitRelation relation) {
    Tile focusTile = relation == UnitRelation.Ally
      ? TileManager.allyFocusTile
      : TileManager.enemyFocusTile;

    List<Tile> shooterSpawns = spawns == allySpawns
      ? allyShooterSpawns
      : enemyShooterSpawns;

    foreach (UnitData data in unitsData) {
      Tile tile = null;

      if (data.isBoss) {
        if (bossSpawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(bossSpawns);
          bossSpawns.Remove(tile);
        }
        if (tile == null && spawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(spawns);
          spawns.Remove(tile);
        }
      } else if (data.type == UnitType.Range) {
        if (shooterSpawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(shooterSpawns);
          shooterSpawns.Remove(tile);
        }
        if (tile == null && spawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(spawns);
          spawns.Remove(tile);
        }
      } else {
        if (spawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(spawns);
          spawns.Remove(tile);
        }
      }

      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return;
      Vector3 center = tile.GetPos();
      Vector3 direction = Vector3.zero;

      if (focusTile != null) {
        direction = (new Vector3(focusTile.GetPos().x, 0, focusTile.GetPos().y) - center).normalized;
        direction.y = 0;
      }

      unit.transform.position = center;
      unit.FromData(data);
      unit.Init(tile, relation, direction);
      QueueManager.Queue.Add(unit);
    }
  }

  private static void SpawnTraps(int enemyTraps) {
    for (int i = 0; i < enemyTraps; i++) {
      Tile tile = TileManager.GetRandomFreeTile();
      if (tile == null) break;
      GameObject trapObj = Instantiate(Instance.hiddenTrapPrefab, tile.transform);
      trapObj.transform.position = tile.GetPos();
      // FIXME: Разные типы ловушек
      trapObj.GetComponent<Trap>().Init(UnitRelation.Enemy, TrapType.BearTrap);
      tile.type = TileType.Trap;
    }
    // FIXME: Союзные ловушки
  }

  private static void InitSupports() {
    List<SupportInstance> allySupports = new ();

    foreach (SupportData data in StateManager.playerSupports) {
      Support unit = Factory.CreateSupportById(data.id);
      if (unit == null) continue;
      SupportInstance support = new(unit, data.level);
      support.FromData(data);
      allySupports.Add(support);
    }

    // FIXME: Саппорты противника
    SupportController.Init(allySupports);
    BattleUI.Instance.UpdateSupports(allySupports);
  }

  public static void Finish() {
    StateManager.WriteUnitsData(
      QueueManager.Queue
        .Where(unit => unit.Relation == UnitRelation.Ally)
        .ToArray(),
      "allies",
      false
    );

    if (battleResult == BattleResult.Victory) {
      foreach (Unit unit in QueueManager.Queue) {
        if (unit.Relation == UnitRelation.Enemy) Reward.Add(unit.killReward);
      }
    }

    StateManager.battleReward = Reward;
    StateManager.battleResult = battleResult;
    string icon = "";
    string text = "";

    switch (battleResult) {
      case BattleResult.Victory:
        icon = "victory";
        text = "Victory!";
        break;
      case BattleResult.Defeat:
        icon = "defeat";
        text = "Defeat!";
        break;
      case BattleResult.Retreat:
        icon = "move";
        text = "Retreat";
        break;
    }

    BattleUI.Instance.DisableUI();
    SceneController.ShowEventInfo(icon, text);
    SceneController.SwitchScene(StateManager.enterScene);
  }
}
