[System.Serializable]
public class TrainingChain {
  public Unit sourceUnit;
  // FIXME: Добавить саппортов
  // public Support sourceSupport;
  public int sourceVillagersCount;

  public Item[] items;
  public Equipment[] equipment;
  public int cost;

  public Unit resultUnit;
  // public Support resultSupport;
}
