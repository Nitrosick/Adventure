
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Calculate {
  private static readonly float dexterityScaleUnit = 2.5f;
  private static readonly float minHitChance = 5f;
  private static readonly float defaultCritChance = 5f;
  private static readonly float minDamage = 0.3f;
  private static readonly float defenseFactor = 10f;
  private static readonly float noFineDistance = 6f;
  private static readonly float distanceFinePerUnit = 4f;

  public static float HitChance(Unit attacker, Unit target) {
    if (target.Effects.HasAnyEffect(new string[] { "Block", "Wall", "Root" })) return 100f;

    float result = attacker.Equip.primary.precision;

    // Equipment weight
    int attackerWeightCoef = attacker.Equip.GetWeightCoefficient();
    int targetWeightCoef = target.Equip.GetWeightCoefficient();
    result -= (attackerWeightCoef - targetWeightCoef) * 5;

    // Terrain
    int atkH = attacker.CurrentTile.height;
    int tarH = target.CurrentTile.height;
    result += (atkH - tarH) * 10;

    // Parameters
    float dexDelta = attacker.Dexterity - target.Dexterity;
    if (dexDelta < 0) result -= Math.Abs(dexDelta) * dexterityScaleUnit;

    // Player abilities
    if (attacker.IsHero) result += AbilityController.PrecisionBonus();
    if (target.IsHero) result -= AbilityController.EvasionBonus();

    // Effects
    if (attacker.Type == UnitType.Range) {
      if (attacker.Effects.HasEffect("Cover") || target.Effects.HasEffect("Cover")) result /= 2;
    }

    // Distance
    float distance = Vector3.Distance(attacker.transform.position, target.transform.position);
    float disDelta = distance - noFineDistance;
    if (disDelta > 0) result -= disDelta * distanceFinePerUnit;

    return result < minHitChance ? minHitChance : result;
  }

  public static float CritModifier(Unit attacker, Unit target) {
    float multiplier = 1f;
    float chance = defaultCritChance;

    // Parameters
    float dexDelta = attacker.Dexterity - target.Dexterity;
    if (dexDelta > 0) chance += dexDelta * dexterityScaleUnit;

    // Player abilities
    if (attacker.IsHero) chance += AbilityController.CritChanceBonus();

    bool success = Utils.RollChance(chance);
    if (success) multiplier = attacker.Equip.primary.critModifier;

    return multiplier;
  }

  public static float Damage(Unit attacker, Unit target) {
    Weapon attackerWeapon = attacker.Equip.primary;
    Armor targetArmor = target.Equip.armor;

    // Armor and weapon
    float resist = target.Equip.GetTotalResists()[attackerWeapon.damageType];
    if (target.IsHero) resist += AbilityController.AllDamageResistBonus();
    float damage = attacker.Equip.GetTotalDamage();
    if (resist != 0) damage *= 1f - (resist / 100f);
    float defense = target.Equip.GetTotalDefense();
    if (attackerWeapon.armorPenetration > 0 && (targetArmor.weight != EquipmentWeight.Light)) {
      defense *= 1f - (attackerWeapon.armorPenetration / 100f);
    }

    float total = damage * Mathf.Exp(-defense / defenseFactor);

    // Player abilities
    if (attacker.IsHero) {
      total *= AbilityController.DamageBonus(attackerWeapon.damageType);
      if (target.IsBoss) total *= AbilityController.DamageVsBossesBonus();
    }

    // Terrain
    int atkH = attacker.CurrentTile.height;
    int tarH = target.CurrentTile.height;
    total *= 1f + (atkH - tarH) * 0.1f;

    // Effects
    float blockMultiplier = 1f;
    if (target.Effects.HasAnyEffect(new string[] { "Block", "Wall" })) {
      if (target.Equip.secondary is Armor shield) {
        blockMultiplier = shield.blockMultiplier;
        if (target.IsHero) blockMultiplier += AbilityController.BlockBonus();
      }
    }
    total /= blockMultiplier;

    return total < minDamage ? minDamage : total;
  }

  public static List<Effect> ItemEffects(Unit attacker, Unit target) {
    List<Effect> result = new();
    Equipment primary = attacker.Equip.primary;
    Equipment secondary = attacker.Equip.secondary;
    Equipment attackerArmor = attacker.Equip.armor;
    Equipment targetArmor = target.Equip.armor;
    Equipment[] items = { primary, secondary, attackerArmor };

    foreach (Equipment item in items) {
      if (item == null || item.effect == null) continue;

      float chance = item.effectChance;
      if (item.effect.effectName == "Bleeding" && targetArmor.weight == EquipmentWeight.Heavy) {
        chance /= 2;
      } else if (item.effect.effectName == "Stun") {
        if (target.IsHero) chance -= AbilityController.StunResist();
        chance += attacker.Strength - target.Strength;
      }
      if (Utils.RollChance(chance)) result.Add(item.effect);
    }

    return result;
  }

  public static List<Skill> ItemPassiveSkills(Unit unit) {
    List<Skill> result = new();
    Equipment primary = unit.Equip.primary;
    Equipment secondary = unit.Equip.secondary;
    Equipment attackerArmor = unit.Equip.armor;
    Equipment[] items = { primary, secondary, attackerArmor };

    foreach (Equipment item in items) {
      if (item == null || item.skill == null || item.skill.isActive) continue;

      float chance = item.skill.activateChance;
      if (item.skill.displayName == "Parry" && unit.IsHero) chance += AbilityController.EvasionBonus();
      if (Utils.RollChance(chance)) result.Add(item.skill);
    }

    return result;
  }
}
