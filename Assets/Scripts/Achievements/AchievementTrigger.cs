using UnityEngine;

public class AchievementTrigger : MonoBehaviour {
  public string id;
  public float value;

  public void Trigger(bool inBattle = false) {
    AchievementManager.UpdateAchievement(id, value, inBattle);
  }
}
