using UnityEngine;

public abstract class Equipment : ScriptableObject {
  public int id;
  public string itemName;
  [TextArea] public string description;

  public EquipmentType type;
  public EquipmentWeight weight;
  public EquipmentRarity rarity;
  // FIXME: Переделать на массив эффектов
  public Effect effect;
  public Skill skill;
  public Sprite icon;
}
