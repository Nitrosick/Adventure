using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
  public static Player Instance;
  public PlayerMove Move { get; private set; }
  public PlayerArmy Army { get; private set; }
  public PlayerInventory Inventory { get; private set; }

  public int Gold { get; private set; }
  public int[] Resources { get; private set; } = { 0, 0, 0, 0 };
  public int Villagers { get; private set; }
  public int MaxVillagers { get; private set; } = 5;
  public int Experience { get; private set; }
  public int Fame { get; private set; }
  public int Level { get; private set; } = 1;
  public int StatPoints { get; private set; } = 1;

  public int MaxFame { get; private set; } = 10000;
  public int MaxLevel { get; private set; } = 30;

  private readonly int baseMaxVillagers = 5;
  private readonly int bonusVillagersPerStep = 5;
  private readonly int fameStepSize = 500;
  private readonly int villagersLimit = 100;

  private void Awake() {
    Instance = this;
    Move = transform.GetComponent<PlayerMove>();
    Army = transform.GetComponent<PlayerArmy>();
    Inventory = transform.GetComponent<PlayerInventory>();

    if (Move == null || Army == null || Inventory == null) {
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
    MapUI.Instance.UpdateResources();
  }

  public void SetResources(int[] array) {
    for (int i = 0; i < array.Length; i++) {
      Resources[i] += array[i];
      if (Resources[i] < 0) Resources[i] = 0;
    }
    StateManager.resources = Resources;
    MapUI.Instance.UpdateResources();
  }

  public void SetVillagers(int value) {
    Villagers += value;
    if (Villagers < 0) Villagers = 0;
    StateManager.villagers = Villagers;
    MapUI.Instance.UpdateResources();
  }

  private void SetMaxVillagers() {
    int bonusUnits = Fame / fameStepSize * bonusVillagersPerStep;
    MaxVillagers = Mathf.Min(baseMaxVillagers + bonusUnits, villagersLimit);
    StateManager.maxVillagers = MaxVillagers;
    MapUI.Instance.UpdateResources();
  }

  public void AddExpirience(int value) {
    Experience += value;
    while (Experience >= XPForNextLevel) {
      Experience -= XPForNextLevel;
      LevelUp();
    }
    StateManager.experience = Experience;
    MapUI.Instance.UpdateResources();
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
    if (Level > 1) SetStatPoints(1);
    Unit hero = Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero != null) hero.LevelUp();
    _ = Toast.Show("star", "Level up!");
    // FIXME: Инфо в альманахе о повышении уровня
    StateManager.level = Level;
    Army.UpdateState();
  }

  public void SetFame(int value) {
    Fame += value;
    if (Fame < 0) Fame = 0;
    SetMaxVillagers();
    StateManager.fame = Fame;
  }

  public void SetStatPoints(int value) {
    StatPoints += value;
    if (StatPoints < 0) StatPoints = 0;
    StateManager.statPoints = StatPoints;
  }

  public int[] GetTotalPeople() {
    // FIXME: Прибавить supports юнитов
    return new int[] { Villagers, Army.Units.Count };
  }

  public void CollectReward(Reward reward) {
    AddExpirience(reward.experience);
    SetFame(reward.fame);
    int goldValue = Utils.GetRandomInRange(reward.goldRange[0], reward.goldRange[1]);
    SetGold(goldValue);
    SetResources(reward.resources);
    Inventory.AddItems(reward.equipment);
    Inventory.AddItems(reward.items);
    MapUI.Instance.UpdateResources();
  }

  private void GetStateData() {
    // FIXME: Перенос данных между локациями
    Gold = StateManager.gold;
    Resources = StateManager.resources;
    Villagers = StateManager.villagers;
    MaxVillagers = StateManager.maxVillagers;
    Experience = StateManager.experience;
    Fame = StateManager.fame;
    Level = StateManager.level;
    StatPoints = StateManager.statPoints;

    if (StateManager.playerUnits.Length > 0) Army.UpdateUnits(StateManager.playerUnits);
    if (StateManager.inventoryEquipment.Length > 0) Inventory.UpdateInventory(StateManager.inventoryEquipment);
    if (StateManager.inventoryItems.Length > 0) Inventory.UpdateInventory(StateManager.inventoryItems);

    MapUI.Instance.UpdateResources();
    MapUI.Instance.UpdateLocation(StateManager.currentScene);

    MapZoneEvent events = Move.CurrentZone.GetComponent<MapZoneEvent>();
    events.CheckEvents(true);

    if (!Move.CurrentZone.TryGetComponent<MapZoneBattle>(out var battleZone)) return;
    BattleResult? result = StateManager.battleResult;
    if (result == null) return;

    MapZoneManager.UpdateAfterBattle(result);
    Reward fixedReward = battleZone.fixedReward;

    switch (result) {
      case BattleResult.Victory:
        Reward reward = StateManager.battleReward;
        if (reward == null) return;
        reward.Add(fixedReward);
        battleZone.fixedReward = new Reward();
        CollectReward(reward);
        break;
      case BattleResult.Defeat:
      case BattleResult.Retreat:
        transform.position = Move.startZone.playerPosition;
        Move.CurrentZone = Move.startZone;
        StateManager.currentPlayerZoneId = Move.CurrentZone.id;
        events.CheckEvents(true);
        _ = CameraController.FocusOn(transform.position, true);
        SetFame(fixedReward.fame / 2 * -1);
        break;
    }

    StateManager.SaveGame();
  }
}
