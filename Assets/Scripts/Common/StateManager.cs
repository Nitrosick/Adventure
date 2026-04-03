using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public static class StateManager {
  public static PrefabDatabase PrefabDatabase;

  [RuntimeInitializeOnLoadMethod]
  static void Init() {
    PrefabDatabase = Resources.Load<PrefabDatabase>("Databases/PrefabDatabase");
  }

  // Static
  private readonly static string[] defaultArmyIds = { "u1", "u2", "u2" };
  public readonly static string[] defaultKnowledge = { "aa1", "aa2", "aa3" };

  // Global
  public static int saveSlot;
  public static float dayTime;
  public static int globalTicks;
  public static HashSet<string> openedWindows = new();

  // Moving between scenes
  public static string enterScene;
  public static UnitData[] enemies;
  public static UnitData[] reinforcement;
  public static int reinforcementRound;
  public static int trapsCount;
  public static TrapType trapType;
  public static BattleResult? battleResult;
  public static Reward battleReward;

  // Player data
  public static string currentScene;
  public static string startPlayerZoneId;
  public static string currentPlayerZoneId;
  public static int gold;
  public static int[] resources;
  public static int villagers;
  public static int maxVillagers;
  public static int experience;
  public static int fame;
  public static int reputation;
  public static int level;
  public static int statPoints;
  public static int abilityPoints;
  public static int supportSlots;
  public static int currentWinStreak;
  public static int totalWinStreak;

  public static Dictionary<string, MapZoneData> zonesState;
  public static HashSet<string> collectedZoneLoot;
  public static HashSet<string> unlockedKnowledge;
  public static HashSet<string> unlockedPassages;
  public static UnitData[] playerUnits;
  public static SupportData[] playerSupports;
  public static QuestData[] quests;
  public static AchievementData[] achievements;
  public static AbilityData[] abilities;
  public static Equipment[] inventoryEquipment;
  public static Item[] inventoryItems;
  public static HashSet<string> playerBuffs;

  public static void ResetTemp() {
    enterScene = "";
    enemies = null;
    reinforcement = null;
    battleResult = null;
    battleReward = null;
    reinforcementRound = 0;
    trapsCount = 0;
  }

  public static void ResetPlayerData() {
    saveSlot = 0;
    dayTime = 720f;
    globalTicks = 1;
    currentScene = "";
    startPlayerZoneId = "6";
    currentPlayerZoneId = "6";
    collectedZoneLoot = new HashSet<string> { };
    unlockedKnowledge = new HashSet<string> { };
    unlockedPassages = new HashSet<string> { };
    gold = 0;
    resources = new int[] { 0, 0, 0, 0 };
    villagers = 0;
    maxVillagers = 5;
    experience = 0;
    fame = 0;
    reputation = 0;
    level = 1;
    statPoints = 0;
    abilityPoints = 0;
    supportSlots = 1;
    currentWinStreak = 0;
    totalWinStreak = 0;

    zonesState = new Dictionary<string, MapZoneData> { };
    playerUnits = new UnitData[] { };
    playerSupports = new SupportData[] { };
    quests = new QuestData[] { };
    achievements = new AchievementData[] { };
    abilities = new AbilityData[] { };
    inventoryEquipment = new Equipment[] { };
    inventoryItems = new Item[] { };
    playerBuffs = new HashSet<string> { };
    ResetTemp();
  }

  public static void WriteUnitsData(Unit[] units, string to) {
    UnitData[] newUnits = units.Select(u => u.ToData()).ToArray();

    switch (to) {
      case "allies":
        playerUnits = newUnits;
        break;
      case "enemies":
        enemies = newUnits;
        break;
      case "reinforcement":
        reinforcement = newUnits;
        break;
    }
  }

  private static TData[] ConvertAndAssign<TInstance, TData>(
    TInstance[] array, out TData[] field)
    where TInstance : IDataConvertible<TData>
  {
    field = array.Select(x => x.ToData()).ToArray();
    return field;
  }

  public static void WriteSupportsData(SupportInstance[] units) =>
    ConvertAndAssign(units, out playerSupports);

  public static void WriteQuestsData(QuestInstance[] array) =>
    ConvertAndAssign(array, out quests);

  public static void WriteAchievementsData(AchievementInstance[] array) =>
    ConvertAndAssign(array, out achievements);

  public static void WriteAbilitiesData(AbilityInstance[] array) =>
    ConvertAndAssign(array, out abilities);

  // Save / Load
  private static string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

  public static void SaveGame() {
    if (saveSlot <= 0) {
      Debug.LogError("Save slot is not specified");
      return;
    }
    SaveData data = GetSaveData();

    // string json = JsonConvert.SerializeObject(data);
    // string encrypted = Encryption.Encrypt(json);
    // File.WriteAllText(GetSavePath(saveSlot), encrypted);

    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
    File.WriteAllText(GetSavePath(saveSlot), json);
  }

  public static SaveData LoadGame(int slot, bool setData = true) {
    string path = GetSavePath(slot);
    if (!File.Exists(path)) return null;

    // string encrypted = File.ReadAllText(path);
    // string json = Encryption.Decrypt(encrypted);
    // SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

    string json = File.ReadAllText(path);
    SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

    if (setData) SetLoadedData(data);
    return data;
  }

  public static bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

  public static void DeleteSave(int slot) {
    string path = GetSavePath(slot);
    if (File.Exists(path)) File.Delete(path);
  }

  public static SaveData GetSaveData() {
    string[] equipIds = inventoryEquipment.Select(e => e.id).ToArray();
    string[] itemIds = inventoryItems.Select(i => i.id).ToArray();
    string scene = SceneManager.GetActiveScene().name;

    SaveData data = new() {
      saveTime = DateTime.Now.ToString(),
      dayTime = dayTime,
      globalTicks = globalTicks,
      currentScene = scene == "Menu" ? "Dunpine village" : scene,
      startPlayerZoneId = startPlayerZoneId,
      currentPlayerZoneId = currentPlayerZoneId,
      collectedZoneLoot = collectedZoneLoot,
      unlockedKnowledge = unlockedKnowledge,
      unlockedPassages = unlockedPassages,
      gold = gold,
      resources = resources,
      villagers = villagers,
      maxVillagers = maxVillagers,
      experience = experience,
      fame = fame,
      reputation = reputation,
      level = level,
      statPoints = statPoints,
      abilityPoints = abilityPoints,
      supportSlots = supportSlots,
      currentWinStreak = currentWinStreak,
      totalWinStreak = totalWinStreak,
      zonesState = zonesState,
      playerUnits = playerUnits,
      quests = quests,
      achievements = achievements,
      abilities = abilities,
      playerSupports = playerSupports,
      inventoryEquipmentIds = equipIds,
      inventoryItemIds = itemIds,
      playerBuffs = playerBuffs
    };
    return data;
  }

  private static void SetLoadedData(SaveData data) {
    dayTime = data.dayTime;
    globalTicks = data.globalTicks;
    currentScene = data.currentScene;
    startPlayerZoneId = data.startPlayerZoneId;
    currentPlayerZoneId = data.currentPlayerZoneId;
    collectedZoneLoot = data.collectedZoneLoot;
    unlockedKnowledge = data.unlockedKnowledge;
    unlockedPassages = data.unlockedPassages;
    gold = data.gold;
    resources = data.resources;
    villagers = data.villagers;
    maxVillagers = data.maxVillagers;
    experience = data.experience;
    fame = data.fame;
    reputation = data.reputation;
    level = data.level;
    statPoints = data.statPoints;
    abilityPoints = data.abilityPoints;
    supportSlots = data.supportSlots;
    currentWinStreak = data.currentWinStreak;
    totalWinStreak = data.totalWinStreak;
    zonesState = data.zonesState;
    playerUnits = data.playerUnits;
    playerSupports = data.playerSupports;
    quests = data.quests;
    achievements = data.achievements;
    abilities = data.abilities;
    inventoryEquipment = Factory.CreateEquipById(data.inventoryEquipmentIds);
    inventoryItems = Factory.CreateItemById(data.inventoryItemIds);
    playerBuffs = data.playerBuffs;
  }

  public static void InitPlayerArmy() {
    List<UnitData> defaultArmy = new() { };

    foreach (string id in defaultArmyIds) {
      Unit prefab = PrefabDatabase.GetPrefab(id, true);
      prefab.InSquad = true;
      prefab.CurrentHealth = prefab.Health.GetMaxHP();
      defaultArmy.Add(prefab.ToData());
    }

    playerUnits = defaultArmy.ToArray();
  }
}
