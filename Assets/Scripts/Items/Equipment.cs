using UnityEngine;

public abstract class Equipment : ScriptableObject {
  public int id;
  public string itemName;

  public EquipmentType type;
  public EquipmentWeight weight;
  // FIXME: Переделать на массив эффектов
  public Effect effect;
  public Skill skill;
  public Sprite icon;
}
