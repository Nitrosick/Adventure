using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StateManager
{
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
  public static int gold;
  public static int[] resources = { 0, 0, 0 };
  public static int villagers;
  // public static int maxVillagers;
  public static int experience;
  public static int fame;
  public static int level;

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

  // public static bool WriteData(string key, object data) {
  //   string json = JsonUtility.ToJson(data);
  //   PlayerPrefs.SetString(key, json);
  //   PlayerPrefs.Save();
  //   return true;
  // }

  // public static object ReadData(string key) {
  //   string json = PlayerPrefs.GetString(key);
  //   if (string.IsNullOrEmpty(json)) return null;
  //   return JsonUtility.FromJson<object>(json);
  // }
}
