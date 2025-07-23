using UnityEngine;

public class MapZoneHome : MapZone {
  [Header("Features")]
  public MapZoneFeature[] features;

  [Header("Healing")]
  public string healerName;
  public MasteryLevel healerLevel;

  public void OpenHomeMenu() {
    if (features.Length < 1) return;
    HomeMenuUI.Open(this);
  }
}
