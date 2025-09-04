using System.Linq;
using UnityEngine;

public class MapZoneManager : MonoBehaviour {
  public static MapZoneManager Instance;

  public Material defaultMaterial;
  public Material highlightMaterial;
  public Material stoneMaterial;
  public Material goldMaterial;
  public static MapZone[] Zones { get; private set; }

  private void Awake() {
    Instance = this;

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
    if (result == BattleResult.Victory) {
      // FIXME: Учесть несколько квестов
      if (
        currentZone.events[0] == MapZoneType.Quest &&
        currentZone.TryGetComponent<MapZoneQuest>(out var questZone)
      ) {
        if (
          questZone.questsList.Count > 0 &&
          questZone.questsList[0].objectiveType == QuestObjective.Fight
        ) {
          QuestManager.CompleteQuest(questZone.questsList[0]);
          questZone.questsList.RemoveAt(0);
        }
      }
      currentZone.UnshiftEvent();
    }
  }

  public static void GetStateData() {
    var state = StateManager.zonesState;
    var visited = StateManager.visitedZones;
    var looted = StateManager.collectedZoneLoot;

    var activeQuests = QuestManager.Instance.database.quests
      .Where(q => q.state == QuestState.Accepted)
      .Select(q => q.objectiveZoneId)
      .ToHashSet();

    foreach (var kvp in state) {
      if (kvp.Value == null) continue;
      var zone = FindById(kvp.Key);
      if (zone == null) continue;
      zone.events = kvp.Value;
    }

    foreach (var zone in Zones.Where(z => visited.Contains(z.id))) {
      zone.ShowPathLines();
      zone.secret = false;
      zone.InitMarker();
    }

    foreach (var loot in FindObjectsOfType<MapLoot>().Where(l => looted.Contains(l.id))) {
      Destroy(loot.gameObject);
    }

    foreach (var zone in Zones.Where(z => activeQuests.Contains(z.id))) {
      if (zone.events.Count == 0 || zone.events[0] != MapZoneType.Quest) zone.events.Insert(0, MapZoneType.Quest);
      zone.SetActive();
    }
  }

}
