
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestSlot : MonoBehaviour {
  private Button button;
  private TextMeshProUGUI level;
  private Quest currentQuest;

  private void Awake() {
    button = transform.GetComponent<Button>();
    level = transform.Find("Level").GetComponent<TextMeshProUGUI>();

    if (button == null || level == null) {
      Debug.LogError("Quest slot components initialization error");
      return;
    }

    button.onClick.AddListener(OpenDialog);
  }

  private void OnDestroy() {
    button.onClick.RemoveListener(OpenDialog);
  }

  public void Init(Quest quest) {
    currentQuest = quest;

    bool available = Player.Instance.Level >= currentQuest.requiredLevel;
    level.text = !available
      ? $"<color=#F61010>{currentQuest.requiredLevel}</color>"
      : currentQuest.requiredLevel.ToString();

    button.interactable = available;
  }

  private void OpenDialog() {
    QuestAcceptDialog.Acception(
      AcceptQuest,
      currentQuest
    );
  }

  private void AcceptQuest(bool accepted) {
    if (!accepted) return;
    // Принять квест
    _ = Toast.Show("success", "Quest accepted");
  }
}
