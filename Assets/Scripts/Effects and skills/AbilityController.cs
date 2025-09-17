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
      .OrderBy(a => int.Parse(a.id[2..]))
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

  private static float CalculateBonus(
    AbilityBonusType type,
    string id,
    float initial,
    Func<float, float, float> apply,
    bool percent = true
  ) {
    float result = initial;
    AbilityInstance[] abilities = GetAbilitiesWithBonus(type);

    foreach (AbilityInstance a in abilities) {
      if (a.data.id != id) continue;
      float value = a.data.effectValues[LevelIndex(a.level) - 1];
      if (percent) value /= 100f;
      result = apply(result, value);
    }

    Debug.Log($"{type}: {result}"); // FIXME: Для отладки
    return result;
  }

  public static float DamageBonus(DamageType damageType) =>
    damageType switch {
      DamageType.Slash => CalculateBonus(AbilityBonusType.Damage, "ab1", 1f, (r, v) => r + v),
      DamageType.Chop => CalculateBonus(AbilityBonusType.Damage, "ab2", 1f, (r, v) => r + v),
      DamageType.Crash => CalculateBonus(AbilityBonusType.Damage, "ab3", 1f, (r, v) => r + v),
      _ => 1f
    };

  public static float EvasionBonus(bool isMultiplier = true) =>
    isMultiplier
      ? CalculateBonus(AbilityBonusType.Evasion, "ab5", 1f, (r, v) => r - v)
      : CalculateBonus(AbilityBonusType.Evasion, "ab5", 0f, (r, v) => r + v * 100, false);

  public static float BlockBonus() => CalculateBonus(AbilityBonusType.Block, "ab4", 0f, (r, v) => v);
  public static float HealBonus() => CalculateBonus(AbilityBonusType.Healing, "ab6", 1f, (r, v) => r + v);
  public static float StunResist() => CalculateBonus(AbilityBonusType.Resist, "ab7", 0f, (r, v) => r + v, false);
  public static float PriorityBonus() => CalculateBonus(AbilityBonusType.Priority, "ab8", 0f, (r, v) => r + v, false);
  public static float AmbushProtectBonus() => CalculateBonus(AbilityBonusType.AmbushProtect, "ab9", 0f, (r, v) => r + v, false);
  public static float XpBonus() => CalculateBonus(AbilityBonusType.Experience, "ab10", 1f, (r, v) => r + v);
  public static float PriceBonus() => CalculateBonus(AbilityBonusType.Prices, "ab11", 1f, (r, v) => r - v);
  public static float ArmorReqBonus() => CalculateBonus(AbilityBonusType.Requirements, "ab12", 0f, (r, v) => r + v, false);
  public static float CritChanceBonus() => CalculateBonus(AbilityBonusType.Crit, "ab13", 0f, (r, v) => r + v, false);

  private static AbilityInstance[] GetAbilitiesWithBonus(AbilityBonusType bonus) {
    return allAbilities
      .Where(a => a.data.bonusType == bonus && a.level != AbilityLevel.No)
      .ToArray();
  }

  private static int LevelIndex(AbilityLevel level) {
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }
}
