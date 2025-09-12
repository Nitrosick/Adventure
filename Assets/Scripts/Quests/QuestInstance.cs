[System.Serializable]
public class QuestInstance {
  public Quest data;
  public QuestState state = QuestState.Inactive;

  public QuestInstance(Quest _data, QuestState _state) {
    data = _data;
    state = _state;
  }

  public QuestData ToData() {
    return new QuestData {
      id = data.id,
      state = state
    };
  }

  public void FromData(QuestData data) {
    state = data.state;
  }
}
