using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapZoneManager : MonoBehaviour {
  public static MapZone[] Zones { get; private set; }

  private void Awake() {
    Zones = GameObject.FindGameObjectsWithTag("MapZone")
      .Select(zone => zone.GetComponent<MapZone>())
      .Where(zone => zone != null)
      .ToArray();

    if (StateManager.zonesState.Count == 0) {
      foreach (MapZone zone in Zones) {
        StateManager.zonesState.Add(zone.id, zone.events);
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

  public static void UpdateAfterBattle(BattleResult? result) {
    if (result == null) return;

    MapZone currentZone = FindById(StateManager.currentPlayerZoneId);

    if (currentZone == null) return;
    if (result == BattleResult.Victory) currentZone.UnshiftEvent();
  }

  public static void GetStateData() {
    Dictionary<int, List<MapZoneType>> state = StateManager.zonesState;

    if (state.Count > 0) {
      foreach (int id in state.Keys) {
        if (state[id] != null) {
          MapZone zone = FindById(id);
          if (zone == null) continue;
          zone.events = state[id];
        }
      }
    }
  }
}
