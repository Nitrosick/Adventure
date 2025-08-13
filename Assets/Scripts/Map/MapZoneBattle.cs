using UnityEngine;

public class MapZoneBattle : MonoBehaviour {
  public Unit[] guard;
  public string battlefieldName;
  public int armySlots;
  public Reward fixedReward;
  public float ambushChance;

  // public void SetCleared() {
  //   if (events.Count < 1) {
  //     guard = new Unit[] { };
  //     battlefieldName = "";
  //     armySlots = 0;
  //   }
  // }
}
