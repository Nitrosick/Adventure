using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Equipment/Additional")]
public class AdditionalItem : Equipment {
  public float bonusValue;
  public bool onlyHero;
  public ItemBonus bonusType;
  public UnitType[] unitTypes;
  public EquipmentType[] allowedWeapon;
}
