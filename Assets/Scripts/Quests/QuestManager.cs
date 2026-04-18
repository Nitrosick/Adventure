using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class QuestManager : MonoBehaviour {
  public static List<QuestInstance> questsList = new ();
  private static Quest currentQuest;
  private static Player player;

  void Start() {
    player = Player.Instance;
    GetStateData();
    InitZoneMarks();
  }

  void OnDestroy() {
    questsList.Clear();
  }

  private static async void InitZoneMarks() {
    await Task.Yield();

    foreach (QuestInstance q in GetActiveQuests()) {
      MapZone zone = MapZoneManager.FindById(q.data.objectiveZoneId);
      if (zone == null) continue;
      // FIXME: Сделать проверку, что зона с восклицательным знаком
      zone.SwitchIconMaterial(true);
    }
  }

  public static void AcceptQuest(Quest quest) {
    if (IsQuestActive(quest.id)) return;
    questsList.Add(new QuestInstance(quest, QuestState.Accepted));

    MapZone zone = player.Move.CurrentZone;
    zone.HideQuestionIcon();

    MapZone objectiveZone = MapZoneManager.FindById(quest.objectiveZoneId);
    // FIXME: Сделать проверку, что зона с восклицательным знаком
    if (objectiveZone != null) objectiveZone.SwitchIconMaterial(true);

    if (
      quest.objectiveType == QuestObjective.Fight &&
      !objectiveZone.events.Contains(MapZoneType.Battle)
    ) {
      objectiveZone.events.Add(MapZoneType.Battle);
    }

    StateManager.WriteQuestsData(questsList.ToArray());
    _ = Toast.Show("success", "Quest accepted");
  }

  public static async void CompleteQuest(Quest quest) {
    QuestInstance questIns = questsList.FirstOrDefault(q => q.data.id == quest.id);
    if (questIns == null || IsQuestCompleted(quest.id)) return;
    questIns.state = QuestState.Completed;
    GiveRewards(quest);

    MapZone objectiveZone = MapZoneManager.FindById(quest.objectiveZoneId);
    // FIXME: Сделать проверку, что зона с восклицательным знаком
    if (objectiveZone != null) objectiveZone.SwitchIconMaterial(false);

    await Task.Yield();
    StateManager.WriteQuestsData(questsList.ToArray());
    StateManager.SaveGame();

    QuestModalUI.Instance.ShowReward(questIns);
    _ = Toast.Show("success", "Quest completed");
  }

  private static void GiveRewards(Quest quest) {
    player.CollectReward(quest.reward);
    if (quest.questZoneUpgrades == null) return;

    foreach (var upgrade in quest.questZoneUpgrades) {
      UpgradeZone(upgrade.zoneId, upgrade.feature);
    }
  }

  private static void UpgradeZone(string id, MapZoneFeature feature) {
    MapZone zone = MapZoneManager.FindById(id);
    if (zone == null) return;
    if (zone.TryGetComponent<MapZoneHub>(out var hub)) {
      hub.AddUpgrade(feature);
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

  public static List<QuestInstance> GetActiveQuests() {
    return questsList.Where(q => IsQuestActive(q.data.id)).ToList();
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

  public static void CheckQuestsInZone(MapZone zone) {
    List<QuestInstance> active = GetActiveQuests();
    if (active.Count == 0) return;

    foreach (QuestInstance q in active) {
      if (q.data.objectiveZoneId != zone.id) continue;

      switch (q.data.objectiveType) {
        case QuestObjective.VisitZone:
          CompleteQuest(q.data);
          return;
        case QuestObjective.BringItem:
          bool check = true;
          PlayerInventory inventory = player.Inventory;
          Equipment[] equip = q.data.objectiveEquipment;
          Item[] items = q.data.objectiveItems;

          if (equip.Length > 0 && !inventory.HasItems(equip, true)) check = false;
          if (items.Length > 0 && !inventory.HasItems(items)) check = false;
          if (!check) return;
          currentQuest = q.data;

          Dialog.Instance.Confirmation(
            HandInItems,
            "Quest items",
            $"Hand in {GetObjectiveItemsDescription(q.data)}?"
          );
          return;
      }
    }
  }

  private static void HandInItems(bool accepted) {
    MapZone zone = player.Move.CurrentZone;
    MapZoneEvent evt = zone.GetComponent<MapZoneEvent>();

    if (!accepted) {
      evt.CheckEvents(ignoreQuest: true);
      currentQuest = null;
      return;
    }

    PlayerInventory inventory = player.Inventory;
    inventory.RemoveItems(currentQuest.objectiveEquipment);
    inventory.RemoveItems(currentQuest.objectiveItems);

    CompleteQuest(currentQuest);
    currentQuest = null;
    evt.CheckEvents();
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
