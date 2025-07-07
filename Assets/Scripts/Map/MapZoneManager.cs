using System.Linq;
using UnityEngine;

public class MapZoneManager : MonoBehaviour
{
  public static MapZone[] Zones { get; private set; }

  private void Awake() {
    Zones = GameObject.FindGameObjectsWithTag("MapZone")
      .Select(zone => zone.GetComponent<MapZone>())
      .Where(zone => zone != null)
      .ToArray();

    // FIXME: Загружить из файла
    if (StateManager.clearedZones.Count == 0) {
      foreach (MapZone zone in Zones) {
        StateManager.clearedZones.Add(zone.id, false);
      }
    }
  }

  private void Start() {
    GetStateData();
  }

  private void OnDestroy() {
    Zones = null;
  }

  public static MapZone FindById(int id) {
    foreach (MapZone zone in Zones) {
      if (zone.id == id) return zone;
    }
    return null;
  }

  private static void GetStateData() {
    BattleResult? result = StateManager.battleResult;
    if (result == null) return;

    MapZone currentZone = FindById(StateManager.currentPlayerZoneId);
    if (currentZone == null) return;

    if (result == BattleResult.Victory) {
      currentZone.SetCleared();
      StateManager.clearedZones[currentZone.id] = true;
    }
  }
}
