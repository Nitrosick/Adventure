using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class StateManager {
  public static PrefabDatabase PrefabDatabase;

  [RuntimeInitializeOnLoadMethod]
  static void Init() {
    PrefabDatabase = Resources.Load<PrefabDatabase>("Databases/PrefabDatabase");
  }

  // Moving between scenes
  public static string enterScene;
  public static UnitData[] allies;
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

  public static Dictionary<int, bool> clearedZones = new();
  // FIXME: Сохранение юнитов в отряде + Инвентарь
  public static UnitData[] reserve = { };

  public static void WriteUnitsData(Unit[] units, string to) {
    if (to == "allies") allies = units.Select(u => u.ToData()).ToArray();
    else if (to == "enemies") enemies = units.Select(u => u.ToData()).ToArray();
    else if (to == "reserve") reserve = units.Select(u => u.ToData()).ToArray();
    else Debug.LogError("Invalid units type");
  }

  public static void Reset() {
    enterScene = "";
    allies = null;
    enemies = null;
    battleResult = null;
    battleReward = null;
  }

  // Save / Load
  private static string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

  public static void SaveGame(SaveData data, int slot) {
    data.saveTime = DateTime.Now.ToString();
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(GetSavePath(slot), json);
  }

  public static SaveData LoadGame(int slot) {
    string path = GetSavePath(slot);
    if (!File.Exists(path)) return null;
    string json = File.ReadAllText(path);
    return JsonUtility.FromJson<SaveData>(json);
  }

  public static bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

  public static void DeleteSave(int slot) {
    string path = GetSavePath(slot);
    if (File.Exists(path)) File.Delete(path);
  }
}
