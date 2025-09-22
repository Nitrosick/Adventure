using System.Linq;
using UnityEngine;

[System.Serializable]
public class AchievementTrigger {
  public string id;
  public float value;
}

public class AchievementTriggers : MonoBehaviour {
  public AchievementTrigger[] triggers;

  public void Trigger(string[] ids, bool inBattle = false) {
    foreach (AchievementTrigger trigger in triggers) {
      if (ids.Contains(trigger.id)) AchievementManager.UpdateAchievement(trigger.id, trigger.value, inBattle);
    }
  }
}
