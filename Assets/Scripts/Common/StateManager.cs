using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public static class StateManager {
  public static PrefabDatabase PrefabDatabase;

  [RuntimeInitializeOnLoadMethod]
  static void Init() {
    PrefabDatabase = Resources.Load<PrefabDatabase>("Databases/PrefabDatabase");
  }

  // Global
  public static int saveSlot;
  private readonly static string[] defaultArmyIds = { "u1", "u2", "u2" };

  // Moving between scenes
  public static string enterScene;
  public static UnitData[] enemies;
  public static BattleResult? battleResult;
  public static BattleReward battleReward;

  // Player data
  public static int currentPlayerZoneId;
  public static int gold;
  public static int[] resources;
  public static int villagers;
  public static int maxVillagers;
  public static int experience;
  public static int fame;
  public static int level;
  public static int statPoints;

  public static Dictionary<int, List<MapZoneType>> zonesState;
  public static UnitData[] playerUnits;
  public static Equipment[] inventoryEquipment;
  public static Item[] inventoryItems;

  public static void ResetTemp() {
    enterScene = "";
    enemies = null;
    battleResult = null;
    battleReward = null;
  }

  public static void ResetPlayerData() {
    saveSlot = 0;
    currentPlayerZoneId = 1;
    gold = 0;
    resources = new int[] { 0, 0, 0, 0 };
    villagers = 0;
    maxVillagers = 5;
    experience = 0;
    fame = 0;
    level = 1;
    statPoints = 0;

    zonesState = new Dictionary<int, List<MapZoneType>> { };
    playerUnits = new UnitData[] { };
    inventoryEquipment = new Equipment[] { };
    inventoryItems = new Item[] { };
    ResetTemp();
  }

  public static void WriteUnitsData(Unit[] units, string to, bool rewrite = true) {
    if (to != "allies" && to != "enemies") {
      Debug.LogError("Invalid units type");
      return;
    }

    UnitData[] newUnits = units.Select(u => u.ToData()).ToArray();

    if (rewrite) {
      if (to == "allies") playerUnits = newUnits;
      else enemies = newUnits;
    }
    else {
      if (to == "allies") {
        UnitData[] reserveUnits = playerUnits.Where(u => !u.inSquad).ToArray();
        playerUnits = newUnits.Concat(reserveUnits).ToArray();
      }
      else enemies = newUnits;
    }
  }

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

    SaveData data = new() {
      // FIXME: Установка имени сохранения
      saveName = "New game",
      saveTime = DateTime.Now.ToString(),
      currentPlayerZoneId = currentPlayerZoneId,
      gold = gold,
      resources = resources,
      villagers = villagers,
      maxVillagers = maxVillagers,
      experience = experience,
      fame = fame,
      level = level,
      statPoints = statPoints,
      zonesState = zonesState,
      playerUnits = playerUnits,
      inventoryEquipmentIds = equipIds,
      inventoryItemIds = itemIds
    };
    return data;
  }

  private static void SetLoadedData(SaveData data) {
    currentPlayerZoneId = data.currentPlayerZoneId;
    gold = data.gold;
    resources = data.resources;
    villagers = data.villagers;
    maxVillagers = data.maxVillagers;
    experience = data.experience;
    fame = data.fame;
    level = data.level;
    statPoints = data.statPoints;
    zonesState = data.zonesState;
    playerUnits = data.playerUnits;
    inventoryEquipment = Factory.CreateEquipById(data.inventoryEquipmentIds);
    inventoryItems = Factory.CreateItemById(data.inventoryItemIds);
  }

  public static void InitPlayerArmy() {
    List<UnitData> defaultArmy = new() { };

    foreach (string id in defaultArmyIds) {
      Unit prefab = PrefabDatabase.GetPrefab(id, true);
      defaultArmy.Add(prefab.ToData());
    }

    playerUnits = defaultArmy.ToArray();
  }
}
