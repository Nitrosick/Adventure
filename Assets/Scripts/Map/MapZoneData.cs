using System.Collections.Generic;

[System.Serializable]
public class MapZoneData {
  public List<MapZoneType> events;
  public bool visited;
  public int collectedAt;
  public MapZoneFeature[] upgrades;
}
