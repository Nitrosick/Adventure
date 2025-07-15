using UnityEngine;

public class MapZoneBattle : MapZone {
  [Header("Battle")]
  public Unit[] guard;
  public string battlefieldName;
  public int armySlots;
  public BattleReward fixedReward;

  public override void SetCleared() {
    base.SetCleared();

    if (events.Count < 1) {
      guard = new Unit[] { };
      battlefieldName = "";
      armySlots = 0;
    }
  }
}
