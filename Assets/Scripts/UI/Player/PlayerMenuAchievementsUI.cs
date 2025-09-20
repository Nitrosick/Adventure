using UnityEngine;

public class PlayerMenuAchievementsUI : MonoBehaviour {
  public static PlayerMenuAchievementsUI Instance;
  public GameObject achievementButton;
  private static Transform panel;

  private void Awake() {
    Instance = this;

    panel = transform.GetComponent<Transform>();

    if (panel == null) {
      Debug.LogError("Achievements UI components initialization error");
    }
  }

  public static void Init() {
    Clear();

    foreach (AchievementInstance a in AchievementManager.achievementsList) {
      GameObject achievementObj = Instantiate(Instance.achievementButton, panel);
      achievementObj.GetComponent<AchievementSlot>().Init(a);
    }
  }

  public static void Clear() {
    foreach (Transform child in panel) Destroy(child.gameObject);
  }
}
