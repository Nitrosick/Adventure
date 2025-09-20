using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementModalUI : ModalRewardUI {
  public static AchievementModalUI Instance;

  private static Button close;
  private static TextMeshProUGUI progress;
  private static AchievementInstance achievement;

  private void Awake() {
    Instance = this;
  }

  private void Init() {
    window = transform.Find("Modals/Achievement");
    base.Init(window);

    close = window.Find("Close").GetComponent<Button>();
    progress = window.Find("Progress/Value").GetComponent<TextMeshProUGUI>();

    if (window == null || close == null || progress == null) {
      Debug.LogError("Achievement dialog components initialization error");
      return;
    }

    close.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    if (close != null) close.onClick.RemoveListener(Close);
  }

  public void Open(AchievementInstance _achievement) {
    Init();
    achievement = _achievement;
    title.text = achievement.data.title;
    if (achievement.completed) title.text += " (Received)";
    text.text = achievement.data.description;
    progress.text = $"{achievement.progress} / {achievement.data.objectiveCount}";
    Reward reward = achievement.data.reward;

    ShowReward(reward);
    RenderSlots(reward, Instance.slotPrefab);

    window.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.OpenWindow("achievement-dialog");
  }

  protected override void Close() {
    base.Close();
    progress.text = "";
    ClearSlots();
    SceneController.CloseWindow("achievement-dialog");
  }
}
