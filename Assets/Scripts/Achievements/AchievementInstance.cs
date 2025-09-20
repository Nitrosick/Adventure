[System.Serializable]
public class AchievementInstance {
  public Achievement data;
  public bool completed;
  public float progress;

  public AchievementInstance(Achievement _data) {
    data = _data;
  }

  public AchievementData ToData() {
    return new AchievementData {
      id = data.id,
      completed = completed,
      progress = progress
    };
  }

  public void FromData(AchievementData data) {
    completed = data.completed;
    progress = data.progress;
  }
}
