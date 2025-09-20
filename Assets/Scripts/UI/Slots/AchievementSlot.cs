using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementSlot : MonoBehaviour {
  private Button button;
  private TextMeshProUGUI title;
  private TextMeshProUGUI progress;
  private GameObject completedMark;
  private AchievementInstance currentAchievement;

  private void Awake() {
    button = transform.GetComponent<Button>();
    title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
    progress = transform.Find("Progress").GetComponent<TextMeshProUGUI>();
    completedMark = transform.Find("Completed").gameObject;

    if (button == null || title == null || progress == null || completedMark == null) {
      Debug.LogError("Achievement slot components initialization error");
      return;
    }

    button.onClick.AddListener(OpenInfo);
  }

  private void OnDestroy() {
    button.onClick.RemoveListener(OpenInfo);
  }

  public void Init(AchievementInstance achievement) {
    currentAchievement = achievement;
    title.text = achievement.data.title;
    progress.text = $"{achievement.progress} / {achievement.data.objectiveCount}";
    progress.gameObject.SetActive(!achievement.completed);
    completedMark.SetActive(achievement.completed);
    // FIXME: Менять вид кнопки когда прогресс = 0
  }

  private void OpenInfo() {
    AchievementModalUI.Instance.Open(currentAchievement);
  }
}
