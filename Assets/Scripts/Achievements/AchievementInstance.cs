[System.Serializable]
public class AchievementInstance : IDataConvertible<AchievementData> {
  public Achievement data;
  public bool completed;
  public float progress;
  public long timestamp;

  public AchievementInstance(Achievement _data) {
    data = _data;
  }

  public AchievementData ToData() {
    return new AchievementData {
      id = data.id,
      completed = completed,
      progress = progress,
      timestamp = timestamp
    };
  }

  public void FromData(AchievementData data) {
    completed = data.completed;
    progress = data.progress;
    timestamp = data.timestamp;
  }
}
