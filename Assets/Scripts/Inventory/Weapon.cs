using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Weapon")]
public class Weapon : Equipment {
  public float damage;
  public float critModifier;
  public float armorPenetration;
  public int range;
  public int attackType = 1;

  public GameObject prefab;
  public DamageType damageType;
  public ShotTrajectory trajectory;
  public CoreStat[] scalingStats;
}
