using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Weapon")]
public class Weapon : Equipment {
  public int damage;
  public float critModifier;
  public float armorPenetration;
  public float effectChance;
  public int range;

  public GameObject prefab;
  public DamageType damageType;
}
