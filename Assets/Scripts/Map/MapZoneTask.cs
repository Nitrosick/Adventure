using UnityEngine;

public class MapZoneTask : MonoBehaviour {
  public Quest quest;
  private MapZone zone;

  void Awake() {
    zone = transform.GetComponent<MapZone>();
  }

  void Start() {
    if (!QuestManager.IsQuestInactive(quest.id)) Reset();
  }

  public void OpenQuestModal() {
    if (quest == null || !QuestManager.IsQuestInactive(quest.id)) return;

    QuestModalUI.Instance.Acception(
      AcceptQuest,
      new QuestInstance(quest, QuestState.Inactive)
    );
  }

  private void AcceptQuest(bool accepted) {
    if (!accepted) return;
    Reset();
    transform.GetComponent<MapZoneEvent>().CheckEvents();
    QuestManager.AcceptQuest(quest);
  }

  private void Reset() {
    zone.RemoveEvent(MapZoneType.Task);
    zone.HideQuestionIcon();
  }
}
