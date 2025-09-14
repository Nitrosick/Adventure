using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
  public string saveTime;
  public string currentScene;
  public string startPlayerZoneId;
  public string currentPlayerZoneId;
  public HashSet<string> visitedZones;
  public HashSet<string> collectedZoneLoot;
  public HashSet<string> unlockedKnowledge;
  public int gold;
  public int[] resources;
  public int villagers;
  public int maxVillagers;
  public int experience;
  public int fame;
  public int level;
  public int statPoints;
  public int abilityPoints;
  public int supportSlots;
  public Dictionary<string, List<MapZoneType>> zonesState;
  public UnitData[] playerUnits;
  public SupportData[] playerSupports;
  public QuestData[] quests;
  public AbilityData[] abilities;
  public string[] inventoryEquipmentIds;
  public string[] inventoryItemIds;
}
