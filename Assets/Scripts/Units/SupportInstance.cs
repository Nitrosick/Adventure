[System.Serializable]
public class SupportInstance {
  public Support data;
  public MasteryLevel level;
  public UnitRelation relation;
  public string effectDescription;
  public bool inSquad;
  public bool isNew;

  public SupportInstance(Support _data, MasteryLevel _level) {
    data = _data;
    level = _level;
    effectDescription = _data.GetEffectDescription(_level);
  }

  public SupportData ToData() {
    return new SupportData {
      id = data.id,
      level = level,
      inSquad = inSquad
    };
  }

  public void FromData(SupportData data) {
    level = data.level;
    inSquad = data.inSquad;
  }
}
