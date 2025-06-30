using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
  public static Player Instance;
  private PlayerMove move;
  public PlayerArmy Army { get; private set; }
  public PlayerInventory Inventory { get; private set; }

  public int Gold { get; private set; }
  public int[] Resources { get; private set; } = { 0, 0, 0 };
  public int Villagers { get; private set; }
  public int MaxVillagers { get; private set; }
  public int Experience { get; private set; }
  public int Fame { get; private set; }
  public int Level { get; private set; } = 1;

  public int MaxFame { get; private set; } = 10000;
  public int MaxLevel { get; private set; } = 30;

  private void Awake() {
    Instance = this;
    move = transform.GetComponent<PlayerMove>();
    Army = transform.GetComponent<PlayerArmy>();
    Inventory = transform.GetComponent<PlayerInventory>();

    if (move == null || Army == null || Inventory == null) {
      Debug.LogError("Player components initialization error");
    }
  }

  private void Start() {
    GetStateData();
  }

  public void SetGold(int value) {
    Gold += value;
    if (Gold < 0) Gold = 0;
    StateManager.gold = Gold;
  }

  public void SetResources(int[] array) {
    for (int i = 0; i < array.Length; i++) {
      Resources[i] += array[i];
      if (Resources[i] < 0) Resources[i] = 0;
    }
    StateManager.resources = Resources;
  }

  public void SetVillagers(int value) {
    Villagers += value;
    if (Villagers < 0) Villagers = 0;
    StateManager.villagers = Villagers;
  }

  public void AddExpirience(int value) {
    Experience += value;
    while (Experience >= XPForNextLevel) {
      Experience -= XPForNextLevel;
      LevelUp();
    }
    StateManager.experience = Experience;
  }

  public int XPForNextLevel => GetXPForLevel(Level);

  private int GetXPForLevel(int lvl) {
    float baseXP = 50f;
    float growthFactor = 1.3f;

    float xp = baseXP * Mathf.Pow(lvl, growthFactor);
    return Mathf.RoundToInt(xp / 10f) * 10;
  }

  private void LevelUp() {
    Level++;
    Debug.Log($"{name} повысил уровень до {Level}!");
    StateManager.level = Level;

    // if (Level % 5 == 0) BoostUnitsLevel();
    // FIXME: Повышение уровня юнитов
  }

  public void SetFame(int value) {
    // FIXME: Рассчет MaxVillagers
    Fame += value;
    if (Fame < 0) Fame = 0;
    StateManager.fame = Fame;
  }

  private void GetStateData() {
    // FIXME: перенос данных между локациями
    BattleResult? result = StateManager.battleResult;
    if (result == null) return;

    Gold = StateManager.gold;
    Resources = StateManager.resources;
    Villagers = StateManager.villagers;
    Experience = StateManager.experience;
    Fame = StateManager.fame;
    Level = StateManager.level;

    UnitData[] units = StateManager.allies;
    UnitData[] reserve = StateManager.reserve;
    UnitData[] allUnit = units.Concat(reserve).ToArray();
    Army.UpdateUnits(allUnit);

    if (result == BattleResult.Defeat) {
      transform.position = move.startZone.playerPosition;
      move.CurrentZone = move.startZone;
      Army.UpdateUnitsHPAfterDefeat();
    } else {
      BattleReward reward = StateManager.battleReward;
      if (reward == null) return;
      SetGold(reward.Gold);
      SetResources(reward.resources);
      AddExpirience(reward.experience);
      Inventory.AddItems(reward.items);
      MapUI.UpdateResources(Gold, Resources, Villagers, MaxVillagers);
    }
  }
}
