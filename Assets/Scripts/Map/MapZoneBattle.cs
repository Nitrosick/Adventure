using UnityEngine;

public class MapZoneBattle : MonoBehaviour {
  public Unit[] guard;
  public Unit[] reinforcement;
  public int reinforcementRound;
  public string battlefieldName;
  public int armySlots;
  public float ambushChance;
  public bool instant = true;
  public int trapsCount;
  public TrapType trapType;
  public Reward fixedReward;

  public void StartBattle() {
    transform.GetComponent<MapZoneEvent>().StartBattle(this);
  }
}
