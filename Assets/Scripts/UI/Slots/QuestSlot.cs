
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestSlot : MonoBehaviour {
  private Button button;
  private TextMeshProUGUI title;
  private TextMeshProUGUI levelLabel;
  private TextMeshProUGUI level;
  private GameObject completedMark;
  private QuestInstance currentQuest;

  private void Awake() {
    button = transform.GetComponent<Button>();
    title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
    levelLabel = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    level = transform.Find("Level").GetComponent<TextMeshProUGUI>();
    completedMark = transform.Find("Completed").gameObject;

    if (button == null || title == null || levelLabel == null || level == null || completedMark == null) {
      Debug.LogError("Quest slot components initialization error");
      return;
    }

    button.onClick.AddListener(OpenDialog);
  }

  private void OnDestroy() {
    button.onClick.RemoveListener(OpenDialog);
  }

  public void Init(QuestInstance quest) {
    currentQuest = quest;
    title.text = quest.data.title;

    bool available = Player.Instance.Level >= quest.data.requiredLevel;
    level.text = !available
      ? $"<color=#F61010>{quest.data.requiredLevel}</color>"
      : quest.data.requiredLevel.ToString();

    levelLabel.gameObject.SetActive(quest.state != QuestState.Completed);
    level.gameObject.SetActive(quest.state != QuestState.Completed);
    completedMark.SetActive(quest.state == QuestState.Completed);
    button.interactable = available;
  }

  private void OpenDialog() {
    QuestModalUI.Instance.Acception(
      AcceptQuest,
      currentQuest
    );
  }

  private void AcceptQuest(bool accepted) {
    if (!accepted) return;
    button.interactable = false;
    QuestManager.AcceptQuest(currentQuest.data);
    _ = Toast.Show("success", "Quest accepted");
  }
}
