using System.Collections.Generic;
using System.Linq;
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
  }

  public static void CompleteQuest(Quest quest) {
    QuestInstance questIns = questsList.FirstOrDefault(q => q.data.id == quest.id);
    if (questIns == null || IsQuestCompleted(quest.id)) return;
    questIns.state = QuestState.Completed;
    GiveRewards(quest);
    QuestModalUI.Instance.ShowReward(questIns);
    StateManager.WriteQuestsData(questsList.ToArray());
  }

  private static void GiveRewards(Quest quest) {
    Player.Instance.CollectReward(quest.reward);
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
