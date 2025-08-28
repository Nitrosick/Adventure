using UnityEngine;

public class QuestsMenuUI : HomeMenuFeature {
  private Transform questsList;
  private GameObject emptyText;
  private Quest[] quests;

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
    return questsList != null && emptyText != null;
  }

  public void Init(string name, MasteryLevel lvl, Quest[] _quests) {
    InitHeader(name, lvl);
    quests = _quests;
    UpdateQuestsData();
  }

  private void UpdateQuestsData() {
    ClearSlots(questsList);

    emptyText.SetActive(quests.Length == 0);

    foreach (Quest quest in quests) {
      GameObject slot = Instantiate(slotPrefab, questsList);
      slot.GetComponent<QuestSlot>().Init(quest);
    }
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(questsList);
  }
}
