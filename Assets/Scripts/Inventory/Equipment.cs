using UnityEngine;

public abstract class Equipment : ScriptableObject {
  public string id;
  public string itemName;
  [TextArea(5, 20)] public string description;
  public int[] requirementStats = { 0, 0, 0 };
  public int requirementLevel = 1;

  public EquipmentType type;
  public UnitEquipSlot slot;
  public EquipmentWeight weight;
  public Rarity rarity;
  // FIXME: Переделать на массив эффектов
  public Effect effect;
  public float effectChance;
  public Skill skill;
  public Sprite icon;
  public int price;
  public bool isNew;
}
