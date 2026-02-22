using UnityEngine;

public class MapZoneBattle : MonoBehaviour {
  public Unit[] guard;
  public Unit[] reinforcement;
  public int reinforcementRound;
  public string battlefieldName;
  public int armySlots;
  public int trapsCount;
  public TrapType trapType;
  public Reward fixedReward;
  public float ambushChance;
  public bool instant = true;

  public void StartBattle() {
    transform.GetComponent<MapZoneEvent>().StartBattle(this);
  }
}
