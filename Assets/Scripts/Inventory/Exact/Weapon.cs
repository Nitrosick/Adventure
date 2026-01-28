using System;
using UnityEngine;

[Serializable]
public class ChargedAttackParams {
  public float hitChancePenalty;
  public float damageBonus;
  public float critBonus;
}

[CreateAssetMenu(menuName = "GameObjects/Equipment/Weapon")]
public class Weapon : Equipment {
  public float damage;
  public float critModifier;
  public float armorPenetration;
  public float precision = 95f;
  public float range;
  public float missleSpeed = 1f;

  public GameObject prefab;
  public DamageType damageType;
  public ShotTrajectory trajectory;
  public CoreStat[] scalingStats;
  public ChargedAttackParams chargedAttackParams;
  public Side hand;
  public GameObject misslePrefab;
}
