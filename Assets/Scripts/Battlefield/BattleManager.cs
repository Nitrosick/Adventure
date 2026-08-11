using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour {
  public static BattleManager Instance;
  public TrapRegistry trapRegistry;
  public GameObject hiddenTrapPrefab;
  public ParticleSystem healEffect;

  private UnitData[] allies;
  private UnitData[] enemies;
  private readonly List<UnitData> reserve = new();

  private List<Tile> allySpawns;
  private List<Tile> allyShooterSpawns;
  private List<Tile> enemySpawns;
  private List<Tile> enemyShooterSpawns;
  private List<Tile> bossSpawns;
  private List<Tile> reinforcementSpawns;

  public BattleResult? battleResult;
  public Reward Reward { get; private set; }
  public GameObject corpsePrefab;

  private readonly int delayTime = 2000;

  void Awake() {
    Instance = this;
    battleResult = null;
    Reward = new Reward();

    allies = StateManager.playerUnits
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
    InitSupports();
    SpawnUnits(allies, allySpawns, UnitRelation.Ally);
    SpawnUnits(enemies, enemySpawns, UnitRelation.Enemy);
    SpawnTraps(StateManager.trapsCount);
  }

  void Start() {
    QueueManager.Instance.Init();
  }

  void OnDestroy() {
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
    reinforcementSpawns = TileManager.GetSpawns(TileSpawnType.Reinforcement);
  }

  private void SpawnUnits(UnitData[] unitsData, List<Tile> spawns, UnitRelation relation) {
    List<Tile> shooterSpawns = spawns == allySpawns
      ? allyShooterSpawns
      : enemyShooterSpawns;

    foreach (UnitData data in unitsData) {
      if (relation == UnitRelation.Ally && (data.currentHealth <= 0 || !data.inSquad)) {
        reserve.Add(data);
        continue;
      }

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
      }
      else if (data.type == UnitType.Range) {
        if (shooterSpawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(shooterSpawns);
          shooterSpawns.Remove(tile);
        }
        if (tile == null && spawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(spawns);
          spawns.Remove(tile);
        }
      }
      else {
        if (spawns.Count > 0) {
          tile = TileManager.GetRandomFreeTile(spawns);
          spawns.Remove(tile);
        }
      }

      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return;

      unit.transform.position = tile.GetPos();
      unit.FromData(data);
      unit.Init(tile, relation);
      if (unit.Type == UnitType.Range && tile.height > 1) unit.BehaviorType = AIBehaviorType.HoldPosition;
      QueueManager.Instance.Queue.Add(unit);
    }
  }

  private void SpawnTraps(int enemyTraps) {
    // TODO: Союзные ловушки

    for (int i = 0; i < enemyTraps; i++) {
      Tile tile = TileManager.GetRandomFreeTile();
      if (tile == null) continue;

      GameObject trapObj = Instantiate(Instance.hiddenTrapPrefab, tile.transform);
      trapObj.transform.position = tile.GetPos();
      trapObj.GetComponent<Trap>().Init(UnitRelation.Enemy, StateManager.trapType);
      tile.type = TileType.Trap;
    }
  }

  private void InitSupports() {
    List<SupportInstance> allySupports = new();

    foreach (SupportData data in StateManager.playerSupports.Where(s => s.inSquad)) {
      Support unit = Factory.CreateSupportById(data.id);
      if (unit == null) continue;
      SupportInstance support = new(unit, data.level);
      support.FromData(data);
      allySupports.Add(support);
    }

    // TODO: Саппорты противника
    SupportController.Init(allySupports);
    BattleUI.Instance.UpdateSupports(allySupports);
  }

  public async Task CheckReinforcement(int round) {
    UnitData[] reinforcement = StateManager.reinforcement;
    int reinforcementRound = StateManager.reinforcementRound;

    if (
      reinforcement == null ||
      reinforcement.Length == 0 ||
      reinforcementRound == 0 ||
      reinforcementRound != round
    ) return;

    _ = Toast.Show("battle", "Reinforcements have arrived!");
    SpawnUnits(reinforcement, reinforcementSpawns, UnitRelation.Enemy);

    Tile focusTile = TileManager.reinforcementFocusTile;
    if (focusTile == null) return;
    _ = CameraController.FocusOn(focusTile.GetPos());
    await Task.Delay(delayTime);
  }

  public async void Finish() {
    await Task.Delay(delayTime / 2);

    StateManager.WriteUnitsData(
      QueueManager.Instance.Queue
        .Where(unit => unit.Relation == UnitRelation.Ally)
        .ToArray(),
      "allies",
      reserve.ToArray()
    );

    if (battleResult == BattleResult.Victory) {
      foreach (Unit unit in QueueManager.Instance.Queue) {
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
