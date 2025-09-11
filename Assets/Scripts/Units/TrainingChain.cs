[System.Serializable]
public class TrainingChain {
  public Unit sourceUnit;
  public Support sourceSupport;
  public MasteryLevel sourceSupportLevel;
  public int sourceVillagersCount;

  public Item[] items;
  public Equipment[] equipment;
  public int cost;

  public Unit resultUnit;
  public Support resultSupport;
  public MasteryLevel resultSupportLevel;
}
