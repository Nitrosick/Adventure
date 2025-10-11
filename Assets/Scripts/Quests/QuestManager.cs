using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class QuestManager : MonoBehaviour {
  public static List<QuestInstance> questsList = new ();

  private void Start() {
    GetStateData();
  }

  private void OnDestroy() {
    questsList.Clear();
  }

  public static void AcceptQuest(Quest quest) {
    if (IsQuestActive(quest.id)) return;
    questsList.Add(new QuestInstance(quest, QuestState.Accepted));
    MapZone zone = MapZoneManager.FindById(quest.objectiveZoneId);
    if (zone != null) zone.ActivateQuest(quest);
    StateManager.WriteQuestsData(questsList.ToArray());
    _ = Toast.Show("success", "Quest accepted");
  }

  public static async void CompleteQuest(Quest quest) {
    QuestInstance questIns = questsList.FirstOrDefault(q => q.data.id == quest.id);
    if (questIns == null || IsQuestCompleted(quest.id)) return;
    questIns.state = QuestState.Completed;
    GiveRewards(quest);
    await Task.Yield();
    StateManager.WriteQuestsData(questsList.ToArray());
    QuestModalUI.Instance.ShowReward(questIns);
    _ = Toast.Show("success", "Quest completed");
  }

  private static void GiveRewards(Quest quest) {
    Player.Instance.CollectReward(quest.reward);
    if (quest.questZoneUpgrades == null) return;
    foreach (var upgrade in quest.questZoneUpgrades) {
      UpgradeZone(upgrade.zoneId, upgrade.feature);
    }
  }

  private static void UpgradeZone(string id, MapZoneFeature feature) {
    MapZone zone = MapZoneManager.FindById(id);
    if (zone == null) return;
    if (zone.TryGetComponent<MapZoneHome>(out var home)) {
      home.AddUpgrade(feature);
    }
  }

  public static bool IsQuestInactive(string id) {
    QuestInstance quest = questsList.FirstOrDefault(q => q.data.id == id);
    return quest == null;
  }

  public static bool IsQuestActive(string id) {
    QuestInstance quest = questsList.FirstOrDefault(q => q.data.id == id);
    if (quest == null) return false;
    return quest.state == QuestState.Accepted;
  }

  public static bool IsQuestCompleted(string id) {
    QuestInstance quest = questsList.FirstOrDefault(q => q.data.id == id);
    if (quest == null) return false;
    return quest.state == QuestState.Completed;
  }

  public static string GetObjectiveItemsDescription(Quest quest) {
    List<string> strings = new() { };
    if (quest.objectiveEquipment.Length > 0) strings.Add(GetQuestItemsDescription(quest.objectiveEquipment));
    if (quest.objectiveItems.Length > 0) strings.Add(GetQuestItemsDescription(quest.objectiveItems));
    return string.Join(", ", strings);
  }

  private static string GetQuestItemsDescription(Item[] items) {
    if (items.Length == 0) return null;
    var grouped = items
      .GroupBy(i => i.id)
      .Select(g => new { Item = g.First(), Count = g.Count() });
    return string.Join(", ", grouped.Select(g => $"<b>{g.Item.itemName} x{g.Count}</b>"));
  }

  private static string GetQuestItemsDescription(Equipment[] items) {
    if (items.Length == 0) return null;
    var grouped = items
      .GroupBy(i => i.id)
      .Select(g => new { Item = g.First(), Count = g.Count() });
    return string.Join(", ", grouped.Select(g => $"<b>{g.Item.itemName} x{g.Count}</b>"));
  }

  private static void GetStateData() {
    QuestData[] questData = StateManager.quests;
    if (questData.Length == 0) return;

    questsList = questData.Select(data => {
      Quest quest = Factory.CreateQuestById(data.id);
      if (quest == null) return null;
      QuestInstance questIns = new(quest, data.state);
      questIns.FromData(data);
      return questIns;
    }).ToList();
  }
}
