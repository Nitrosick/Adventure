using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Weapon")]
public class Weapon : Equipment {
  public float damage;
  public float critModifier;
  public float armorPenetration;
  public float effectChance;
  public int range;
  public int attackType = 1;

  public GameObject prefab;
  public DamageType damageType;
  public CoreStat[] scalingStats;
}
