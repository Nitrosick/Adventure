using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
  public string saveName;
  public string saveTime;
  public int currentPlayerZoneId;
  public int gold;
  public int[] resources;
  public int villagers;
  public int maxVillagers;
  public int experience;
  public int fame;
  public int level;
  public int statPoints;
  public Dictionary<int, List<MapZoneType>> zonesState;
  public UnitData[] playerUnits;
  public string[] inventoryEquipmentIds;
  public string[] inventoryItemIds;
}
