using UnityEngine;

public class MapZoneBuilding : MonoBehaviour {
  private MapZone zone;
  public Building building;
  public Requirements requirements;
  public Sprite sprite;
  public BlockedPath[] unlockPathes = {};

  void Awake() {
    zone = transform.GetComponent<MapZone>();

    if (zone == null) {
      Debug.LogError("Map zone building components initialization error");
    }
  }

  public void OpenBuildingPanel() {
    BuildingUI.Open(this);
  }

  public void Remove() {
    if (building == Building.Watchtower) zone.RemoveEvent(MapZoneType.Battle);
    zone.RemoveEvent(MapZoneType.Constructing);
    transform.GetComponent<MapZoneEvent>().CheckEvents();
  }
}
