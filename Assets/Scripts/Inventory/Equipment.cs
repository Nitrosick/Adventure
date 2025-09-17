using System;
using System.Linq;
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

  public int GetPrice() {
    return (int)Math.Round(price * AbilityController.PriceBonus());
  }

  public int[] GetRequirementStats() {
    int[] result = requirementStats.ToArray();
    int abilityEffect = (int)AbilityController.ArmorReqBonus();
    if (type != EquipmentType.Armor || abilityEffect == 0f) return result;
    return result
      .Select(s => s - abilityEffect < 0 ? 0 : s - abilityEffect)
      .ToArray();
  }
}
