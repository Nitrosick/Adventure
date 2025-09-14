using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AbilityController {
  public static List<AbilityInstance> allAbilities = new();

  public static void Init() {
    allAbilities.Clear();

    Ability[] abilitiesData = Resources.LoadAll<Ability>("Abilities");
    allAbilities = abilitiesData
      .Select(a => new AbilityInstance(a, AbilityLevel.No))
      .ToList();

    AbilityData[] savedData = StateManager.abilities;
    foreach (AbilityData ability in savedData) {
      AbilityInstance abilityIns = allAbilities.FirstOrDefault(a => a.data.id == ability.id);
      if (abilityIns == null) continue;
      abilityIns.level = ability.level;
    }
  }

  public static void Learn(string id) {
    AbilityInstance ability = allAbilities.FirstOrDefault(a => a.data.id == id);
    if (ability == null || ability.level == AbilityLevel.Gold) return;
    ability.level = (AbilityLevel)Mathf.Min((int)ability.level + 1, System.Enum.GetValues(typeof(AbilityLevel)).Length - 1);
    Player.Instance.SetAbilityPoints(-1);
    StateManager.WriteAbilitiesData(allAbilities.ToArray());
  }

  public static float DamageBonus(DamageType damageType) {
    float result = 1f;
    AbilityInstance[] abilities = GetAbilitiesWithBonus(AbilityBonusType.DamagePercent);

    foreach (AbilityInstance a in abilities) {
      float value = a.data.effectValues[LevelIndex(a.level) - 1] / 100;
      if (a.data.id == "ab1" && damageType == DamageType.Slash) result += value;
      else if (a.data.id == "ab2" && damageType == DamageType.Chop) result += value;
      else if (a.data.id == "ab3" && damageType == DamageType.Crash) result += value;
    }

    return result;
  }

  public static float BlockBonus() {
    float result = 0;
    AbilityInstance[] abilities = GetAbilitiesWithBonus(AbilityBonusType.BlockPercent);

    foreach (AbilityInstance a in abilities) {
      float value = a.data.effectValues[LevelIndex(a.level) - 1] / 100;
      if (a.data.id == "ab4") result = value;
    }

    return result;
  }

  public static float EvasionBonus(bool isMultiplier = true) {
    float result = isMultiplier ? 1f : 0f;
    AbilityInstance[] abilities = GetAbilitiesWithBonus(AbilityBonusType.Evasion);

    foreach (AbilityInstance a in abilities) {
      float value = a.data.effectValues[LevelIndex(a.level) - 1] / 100;
      if (a.data.id == "ab5") {
        if (isMultiplier) result -= value;
        else result += a.data.effectValues[LevelIndex(a.level) - 1];
      }
    }

    return result;
  }

  public static float HealBonus() {
    float result = 1f;
    AbilityInstance[] abilities = GetAbilitiesWithBonus(AbilityBonusType.Healing);

    foreach (AbilityInstance a in abilities) {
      float value = a.data.effectValues[LevelIndex(a.level) - 1] / 100;
      if (a.data.id == "ab6") result += value;
    }

    return result;
  }

  private static AbilityInstance[] GetAbilitiesWithBonus(AbilityBonusType bonus) {
    return allAbilities
      .Where(a => a.data.bonusType == bonus && a.level != AbilityLevel.No)
      .ToArray();
  }

  private static int LevelIndex(AbilityLevel level) {
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }
}
