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

  // Moving between scenes
  public static string enterScene;
  public static UnitData[] enemies;
  public static BattleResult? battleResult;
  public static BattleReward battleReward;

  // Global
  public static int currentPlayerZoneId;
  public static int gold = 0;
  public static int[] resources = { 0, 0, 0, 0 };
  public static int villagers = 0;
  public static int maxVillagers = 5;
  public static int experience = 0;
  public static int fame = 0;
  public static int level = 1;
  public static int statPoints = 0;

  public static Dictionary<int, List<MapZoneType>> zonesState = new();
  public static UnitData[] playerUnits = { };
  public static Equipment[] inventoryEquipment = { };

  public static void WriteUnitsData(Unit[] units, string to, bool rewrite = true) {
    if (to != "allies" && to != "enemies") {
      Debug.LogError("Invalid units type");
      return;
    }

    UnitData[] newUnits = units.Select(u => u.ToData()).ToArray();

    if (rewrite) {
      if (to == "allies") playerUnits = newUnits;
      else enemies = newUnits;
    } else {
      if (to == "allies") {
        UnitData[] reserveUnits = playerUnits.Where(u => !u.inSquad).ToArray();
        playerUnits = newUnits.Concat(reserveUnits).ToArray();
      }
      else enemies = newUnits;
    }
  }

  public static void Reset() {
    enterScene = "";
    enemies = null;
    battleResult = null;
    battleReward = null;
  }

  // Save / Load
  private static string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

  public static void SaveGame(SaveData data, int slot) {
    data.saveTime = DateTime.Now.ToString();
    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
    File.WriteAllText(GetSavePath(slot), json);
  }

  public static SaveData LoadGame(int slot) {
    string path = GetSavePath(slot);
    if (!File.Exists(path)) return null;
    string json = File.ReadAllText(path);
    return JsonConvert.DeserializeObject<SaveData>(json);
  }

  public static bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

  public static void DeleteSave(int slot) {
    string path = GetSavePath(slot);
    if (File.Exists(path)) File.Delete(path);
  }
}
