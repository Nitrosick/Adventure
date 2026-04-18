using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapZoneManager : MonoBehaviour {
  public static MapZoneManager Instance;

  public Material highlightMaterial;
  public static MapZone[] Zones { get; private set; }

  void Awake() {
    Instance = this;

    Zones = GameObject.FindGameObjectsWithTag("MapZone")
      .Select(zone => zone.GetComponent<MapZone>())
      .Where(zone => zone != null)
      .ToArray();
  }

  void Start() {
    GetStateData();
  }

  void OnDestroy() {
    Zones = null;
  }

  public static MapZone FindById(string id) {
    return Zones.FirstOrDefault(z => z.id == id);
  }

  public static void UpdateAfterBattle(BattleResult? result) {
    MapZone zone = FindById(StateManager.currentPlayerZoneId);
    if (result == null || zone == null || result != BattleResult.Victory) return;

    List<QuestInstance> quests = QuestManager.GetActiveQuests()
      .Where(q => q.data.objectiveZoneId == zone.id && q.data.objectiveType == QuestObjective.Fight)
      .ToList();

    foreach (QuestInstance quest in quests) {
      QuestManager.CompleteQuest(quest.data);
    }

    zone.RemoveEvent(MapZoneType.Battle);
  }

  public static void GetStateData() {
    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    List<string> visited = new() { };

    // Zone events and Hub upgrades
    foreach (var kvp in state) {
      if (kvp.Value == null) continue;
      if (kvp.Value.visited) visited.Add(kvp.Key);

      var zone = FindById(kvp.Key);
      if (zone == null) continue;

      zone.events = kvp.Value.events;

      if (kvp.Value.upgrades != null && zone.TryGetComponent<MapZoneHub>(out var hub)) {
        hub.Upgrades = kvp.Value.upgrades.ToList();
      }
    }

    // Visited zones
    foreach (MapZone zone in Zones.Where(z => visited.Contains(z.id))) {
      zone.ShowPathLines();
      zone.secret = false;
      zone.ResetMarker();
    }

    // Blocked pathes
    foreach (BlockedPath path in FindObjectsOfType<BlockedPath>()) {
      if (StateManager.unlockedPassages.Contains(path.id)) path.Unlock();
      else path.Init();
    }

    // Picked loot
    var looted = StateManager.collectedZoneLoot;
    foreach (MapLoot loot in FindObjectsOfType<MapLoot>().Where(l => looted.Contains(l.id))) {
      Destroy(loot.gameObject);
    }

    // Zone upgrades
    foreach (QuestInstance quest in QuestManager.questsList) {
      if (!QuestManager.IsQuestCompleted(quest.data.id)) continue;
      if (quest.data.questZoneUpgrades == null) continue;

      foreach (var upgrade in quest.data.questZoneUpgrades) {
        MapZone zone = FindById(upgrade.zoneId);
        if (zone == null) continue;

        if (zone.TryGetComponent<MapZoneHub>(out var hub)) {
          hub.AddUpgrade(upgrade.feature);
        }
      }
    }

    // Collecting
    foreach (MapZone zone in Zones.Where(z => z.events.Contains(MapZoneType.Collecting))) {
      if (!state.ContainsKey(zone.id)) continue;
      MapZoneCollecting collecting = zone.GetComponent<MapZoneCollecting>();
      collecting.CollectedAt = state[zone.id].collectedAt;

      if (collecting.CollectedAt > 0) {
        zone.SwitchInteractiveObjects();
        if (StateManager.globalTicks < collecting.CollectedAt + collecting.respawn) zone.SwitchIconMaterial(false);
      } else {
        zone.SwitchIconMaterial(true);
      }
    }
  }

}
