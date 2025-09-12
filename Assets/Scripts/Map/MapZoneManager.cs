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

  public static MapZone FindById(string id) {
    foreach (MapZone zone in Zones) {
      if (zone.id == id) return zone;
    }
    return null;
  }

  public static void UpdateAfterBattle(BattleResult? result) {
    MapZone currentZone = FindById(StateManager.currentPlayerZoneId);
    if (result == null || currentZone == null || result != BattleResult.Victory) return;

    if (
      currentZone.events[0] == MapZoneType.Quest &&
      currentZone.TryGetComponent<MapZoneQuest>(out var questZone) &&
      questZone.questsList.Count > 0
    ) {
      Quest quest = questZone.questsList.FirstOrDefault(q => q.objectiveType == QuestObjective.Fight);
      if (quest != null) {
        QuestManager.CompleteQuest(quest);
        questZone.questsList.Remove(quest);
        if (questZone.questsList.Count == 0) currentZone.UnshiftEvent();
        return;
      }
    }
    currentZone.UnshiftEvent();
  }

  public static void GetStateData() {
    // Zone events
    var state = StateManager.zonesState;
    foreach (var kvp in state) {
      if (kvp.Value == null) continue;
      var zone = FindById(kvp.Key);
      if (zone == null) continue;
      zone.events = kvp.Value;
    }

    // Visited zones
    var visited = StateManager.visitedZones;
    foreach (MapZone zone in Zones.Where(z => visited.Contains(z.id))) {
      zone.ShowPathLines();
      zone.secret = false;
      zone.InitMarker();
    }

    // Picked loot
    var looted = StateManager.collectedZoneLoot;
    foreach (MapLoot loot in FindObjectsOfType<MapLoot>().Where(l => looted.Contains(l.id))) {
      Destroy(loot.gameObject);
    }

    // Zone quests
    var activeQuestsZoneIds = QuestManager.questsList
      .Where(q => q.state == QuestState.Accepted)
      .Select(q => q.data.objectiveZoneId)
      .ToHashSet();

    var activeQuestsIds = QuestManager.questsList
      .Where(q => activeQuestsZoneIds.Contains(q.data.objectiveZoneId))
      .Select(q => q.data.id)
      .ToHashSet();

    foreach (MapZone zone in Zones.Where(z => activeQuestsZoneIds.Contains(z.id))) {
      if (zone.events.Count == 0) zone.events.Add(MapZoneType.Quest);
      else if (zone.events[0] != MapZoneType.Quest) zone.events.Insert(0, MapZoneType.Quest);
      if (!zone.TryGetComponent<MapZoneQuest>(out var zoneQuests)) return;

      foreach (string id in activeQuestsIds) {
        if (zoneQuests.questsList.Any(q => q.id == id)) continue;
        zoneQuests.questsList.Add(Factory.CreateQuestById(id));
      }

      zone.SetActive();
    }
  }

}
