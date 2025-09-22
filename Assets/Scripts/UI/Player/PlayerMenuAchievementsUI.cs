using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuAchievementsUI : MonoBehaviour {
  public static PlayerMenuAchievementsUI Instance;
  public GameObject achievementButton;
  private static Transform panel;
  private static AchievementsFilter filter;
  private static Button inProgressFilter;
  private static Button completedFilter;
  private static Button lockedFilter;

  public enum AchievementsFilter {
    InProgress,
    Completed,
    Locked
  }

  private void Awake() {
    Instance = this;

    panel = transform.Find("Achievements").GetComponent<Transform>();
    Transform filters = transform.Find("AchievementsHeader/Filters").GetComponent<Transform>();
    inProgressFilter = filters.Find("Progress").GetComponent<Button>();
    completedFilter = filters.Find("Completed").GetComponent<Button>();
    lockedFilter = filters.Find("Locked").GetComponent<Button>();

    if (panel == null || inProgressFilter == null || completedFilter == null || lockedFilter == null) {
      Debug.LogError("Achievements UI components initialization error");
      return;
    }

    inProgressFilter.onClick.AddListener(() => FilterItems(AchievementsFilter.InProgress));
    completedFilter.onClick.AddListener(() => FilterItems(AchievementsFilter.Completed));
    lockedFilter.onClick.AddListener(() => FilterItems(AchievementsFilter.Locked));
  }

  private void OnDestroy() {
    inProgressFilter.onClick.RemoveListener(() => {});
    completedFilter.onClick.RemoveListener(() => {});
    lockedFilter.onClick.RemoveListener(() => {});
  }

  public static void Init() {
    List<AchievementInstance> list = AchievementManager.achievementsList;
    AchievementInstance[] inProgress = list.Where(a => a.progress > 0 && !a.completed).ToArray();
    AchievementInstance[] completed = list.Where(a => a.completed).ToArray();
    AchievementInstance[] locked = list.Where(a => a.progress == 0).ToArray();

    inProgressFilter.interactable = inProgress.Length > 0;
    completedFilter.interactable = completed.Length > 0;
    lockedFilter.interactable = locked.Length > 0;

    FilterItems(
      inProgress.Length == 0
        ? AchievementsFilter.Locked
        : AchievementsFilter.InProgress,
      true
    );
  }

  private static void FilterItems(AchievementsFilter value, bool force = false) {
    if (!force && value == filter) return;
    filter = value;
    Clear();

    AchievementInstance[] list = AchievementManager.achievementsList
      .Where(a => {
        return value switch {
          AchievementsFilter.InProgress => a.progress > 0 && !a.completed,
          AchievementsFilter.Completed => a.completed,
          AchievementsFilter.Locked => a.progress == 0,
          _ => false,
        };
      })
      .ToArray();

    foreach (AchievementInstance a in list) {
      GameObject achievementObj = Instantiate(Instance.achievementButton, panel);
      achievementObj.GetComponent<AchievementSlot>().Init(a);
    }
  }

  public static void Clear() {
    foreach (Transform child in panel) Destroy(child.gameObject);
  }
}
