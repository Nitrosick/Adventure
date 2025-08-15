using UnityEngine;

public class MapZoneBuilding : MonoBehaviour {
  public Building building;
  public Requirements requirements;
  public Sprite sprite;

  public void OpenBuildingPanel() {
    BuildingUI.Open(this);
  }
}
