using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
  public string saveTime;
  public int globalTicks;
  public string currentScene;
  public string startPlayerZoneId;
  public string currentPlayerZoneId;
  public int gold;
  public int[] resources;
  public int villagers;
  public int maxVillagers;
  public int experience;
  public int fame;
  public int reputation;
  public int level;
  public int statPoints;
  public int abilityPoints;
  public int supportSlots;
  public int currentWinStreak;
  public int totalWinStreak;
  public string[] inventoryEquipmentIds;
  public string[] inventoryItemIds;

  public HashSet<string> collectedZoneLoot;
  public HashSet<string> unlockedKnowledge;
  public HashSet<string> unlockedPassages;
  public Dictionary<string, MapZoneData> zonesState;
  public UnitData[] playerUnits;
  public SupportData[] playerSupports;
  public QuestData[] quests;
  public AchievementData[] achievements;
  public AbilityData[] abilities;
}
