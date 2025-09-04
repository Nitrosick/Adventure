using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour {
  public static QuestManager Instance;
  public QuestsDatabase database;

  private void Awake() {
    Instance = this;
  }

  private void Start() {
    GetStateData();
  }

  public static void AcceptQuest(Quest quest) {
    if (IsQuestActive(quest.id)) return;
    quest.state = QuestState.Accepted;
    MapZone zone = MapZoneManager.FindById(quest.objectiveZoneId);
    if (zone != null) zone.ActivateQuest();
    // FIXME: Активировать конкретный квест
    // FIXME: Квест может быть за пределами зоны
    StateManager.activeQuests.Add(quest.id);
  }

  public static void CompleteQuest(Quest quest) {
    if (IsQuestCompleted(quest.id)) return;
    quest.state = QuestState.Completed;
    GiveRewards(quest);
    _ = Toast.Show("success", "Quest completed!");
    // FIXME: Выводить сообщение с наградой за квест
    StateManager.activeQuests.Remove(quest.id);
    StateManager.completedQuests.Add(quest.id);
  }

  private static void GiveRewards(Quest quest) {
    Player.Instance.CollectReward(quest.reward);
  }

  public static bool IsQuestActive(string id) {
    Quest quest = Instance.database.quests
      .FirstOrDefault(q => q.id == id);

    if (quest == null) return false;
    return quest.state == QuestState.Accepted;
  }

  public static bool IsQuestCompleted(string id) {
    Quest quest = Instance.database.quests
      .FirstOrDefault(q => q.id == id);

    if (quest == null) return false;
    return quest.state == QuestState.Completed;
  }

  public static void CheckCurrentQuests(QuestObjective type) {
    // FIXME: Вызывать проверку при получении предметов и посещении зон
  }

  private static void GetStateData() {
    HashSet<string> active = StateManager.activeQuests;
    HashSet<string> completed = StateManager.completedQuests;

    foreach (Quest q in Instance.database.quests) {
      if (active.Contains(q.id)) q.state = QuestState.Accepted;
      else if (completed.Contains(q.id)) q.state = QuestState.Completed;
    }
  }
}
