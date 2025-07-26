using UnityEngine;

public class MapZoneHome : MapZone {
  [Header("Features")]
  public MapZoneFeature[] features;

  [Header("Healing")]
  public string healerName;
  public MasteryLevel healerLevel;

  [Header("Trading")]
  public string merchantName;
  public MasteryLevel merchantLevel;
  public bool resourcesSale;
  public Equipment[] equipmentGoods;
  public Item[] itemGoods;

  public void OpenHomeMenu() {
    if (features.Length < 1) return;
    HomeMenuUI.Open(this);
  }
}
