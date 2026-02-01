using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerMenuUIAchievements : MonoBehaviour {
  public static PlayerMenuUIAchievements Instance;
  public GameObject achievementButton;
  private static Transform panel;
  private static AchievementsFilter filter;
  private static Button inProgressFilter;
  private static Button completedFilter;
  private static Button lockedFilter;

  private readonly Dictionary<Button, UnityAction> actions = new();

  public enum AchievementsFilter {
    InProgress,
    Completed,
    Locked
  }

  void Awake() {
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

    actions[inProgressFilter] = () => FilterItems(AchievementsFilter.InProgress);
    actions[completedFilter] = () => FilterItems(AchievementsFilter.Completed);
    actions[lockedFilter] = () => FilterItems(AchievementsFilter.Locked);

    foreach (var pair in actions) pair.Key.onClick.AddListener(pair.Value);
  }

  void OnDestroy() {
    foreach (var pair in actions) pair.Key.onClick.RemoveListener(pair.Value);
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

    AchievementInstance[] sorted = list;
    if (value == AchievementsFilter.InProgress) sorted = list
      .OrderByDescending(a => 100 / a.data.objectiveCount * a.progress)
      .ToArray();
    else if (value == AchievementsFilter.Completed) sorted = list
      .OrderByDescending(a => a.timestamp)
      .ToArray();

    foreach (AchievementInstance a in sorted) {
      GameObject achievementObj = Instantiate(Instance.achievementButton, panel);
      achievementObj.GetComponent<AchievementSlot>().Init(a);
    }
  }

  public static void Clear() {
    foreach (Transform child in panel) Destroy(child.gameObject);
  }
}
