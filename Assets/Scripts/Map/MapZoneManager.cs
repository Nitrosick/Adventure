using System.Collections.Generic;
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
    MapZone z = FindById(StateManager.currentPlayerZoneId);
    if (result == null || z == null || result != BattleResult.Victory) return;

    if (
      z.events[0] == MapZoneType.Quest &&
      z.QuestsList.Count > 0
    ) {
      Quest quest = z.QuestsList.FirstOrDefault(q => q.objectiveType == QuestObjective.Fight);
      if (quest != null) {
        QuestManager.CompleteQuest(quest);
        z.QuestsList.Remove(quest);
        if (z.QuestsList.Count == 0) z.RemoveEvent(MapZoneType.Quest);
        return;
      }
    }
    z.RemoveEvent(MapZoneType.Battle);
  }

  public static void GetStateData() {
    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    List<string> visited = new() { };

    // Zone events and Home upgrades
    foreach (var kvp in state) {
      if (kvp.Value == null) continue;
      if (kvp.Value.visited) visited.Add(kvp.Key);

      var zone = FindById(kvp.Key);
      if (zone == null) continue;

      zone.events = kvp.Value.events;

      if (kvp.Value.upgrades != null && zone.TryGetComponent<MapZoneHome>(out var home)) {
        home.Upgrades = kvp.Value.upgrades.ToList();
      }
    }

    // Visited zones
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

      foreach (string id in activeQuestsIds) {
        if (zone.QuestsList.Any(q => q.id == id)) continue;
        zone.QuestsList.Add(Factory.CreateQuestById(id));
      }

      zone.SetActive();
    }

    // Collecting
    foreach (MapZone zone in Zones.Where(z => z.events.Contains(MapZoneType.Collecting))) {
      if (!state.ContainsKey(zone.id)) continue;
      MapZoneCollecting collecting = zone.GetComponent<MapZoneCollecting>();
      collecting.CollectedAt = state[zone.id].collectedAt;

      if (collecting.CollectedAt > 0 && StateManager.globalTicks < collecting.CollectedAt + collecting.respawn) {
        zone.SwitchIcon(false);
        zone.isEmpty = true;
      }
      else {
        zone.SwitchIcon(true);
        zone.isEmpty = false;
      }
    }
  }

}
