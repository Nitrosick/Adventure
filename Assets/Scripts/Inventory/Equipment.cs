using UnityEngine;

public abstract class Equipment : ScriptableObject {
  public string id;
  public string itemName;
  [TextArea] public string description;
  public int[] requirementStats = { 0, 0, 0 };
  public int requirementLevel = 1;

  public EquipmentType type;
  public UnitEquipSlot slot;
  public EquipmentWeight weight;
  public Rarity rarity;
  // FIXME: Переделать на массив эффектов
  public Effect effect;
  public Skill skill;
  public Sprite icon;
  public int price;
}
