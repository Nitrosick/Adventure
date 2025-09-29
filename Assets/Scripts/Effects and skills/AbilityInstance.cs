[System.Serializable]
public class AbilityInstance : IDataConvertible<AbilityData> {
  public Ability data;
  public AbilityLevel level;

  public AbilityInstance(Ability _data, AbilityLevel _level) {
    data = _data;
    level = _level;
  }

  public AbilityData ToData() {
    return new AbilityData {
      id = data.id,
      level = level,
    };
  }

  public void FromData(AbilityData data) {
    level = data.level;
  }
}
