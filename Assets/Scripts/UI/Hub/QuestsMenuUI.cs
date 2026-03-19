using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestsMenuUI : HubMenuFeature {
  private Transform questsList;
  private GameObject emptyText;
  private List<QuestInstance> quests;

  protected override void Awake() {
    base.Awake();

    questsList = transform.Find("List");
    emptyText = transform.Find("Empty").gameObject;

    if (!ComponentsInitialized()) {
      Debug.LogError("Quests menu UI components initialization error");
      return;
    }
  }

  private bool ComponentsInitialized() {
    return new object[] {
      questsList, emptyText
    }.All(x => x != null);
  }

  public void Init(string name, MasteryLevel lvl, Quest[] _quests) {
    InitHeader(name, lvl);
    List<QuestInstance> questsList = new();

    foreach (Quest q in _quests) {
      if (!QuestManager.IsQuestInactive(q.id)) continue;
      questsList.Add(new QuestInstance(q, QuestState.Inactive));
    }

    quests = questsList;
    UpdateQuestsData();
  }

  void UpdateQuestsData() {
    ClearSlots(questsList);

    emptyText.SetActive(quests.Count == 0);

    foreach (QuestInstance quest in quests) {
      GameObject slot = Instantiate(GameManager.I.slotQuest, questsList);
      slot.GetComponent<QuestSlot>().Init(quest);
    }
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(questsList);
  }
}
