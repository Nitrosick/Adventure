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
  public Side hand;
  public GameObject prefab;
  public DamageType damageType;
  public CoreStat[] scalingStats;

  public float damage;
  public float critModifier;
  public float range;
  public float armorPenetration;
  public float precision = 95f;

  public ChargedAttackParams chargedAttackParams;

  public float missleSpeed = 1f;
  public ShotTrajectory trajectory;
  public GameObject misslePrefab;
}
