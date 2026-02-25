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
    ability.level = (AbilityLevel)Mathf.Min((int)ability.level + 1, Enum.GetValues(typeof(AbilityLevel)).Length - 1);
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

    // Debug.Log($"{type}: {result}"); // FIXME: Для отладки
    return result;
  }

  public static float DamageBonus(DamageType damageType) =>
    damageType switch {
      DamageType.Slash => CalculateBonus(AbilityBonusType.Damage, "ab1", 1f, (r, v) => r + v),
      DamageType.Chop => CalculateBonus(AbilityBonusType.Damage, "ab2", 1f, (r, v) => r + v),
      DamageType.Crash => CalculateBonus(AbilityBonusType.Damage, "ab3", 1f, (r, v) => r + v),
      _ => 1f
    };

  public static float BlockBonus() => CalculateBonus(AbilityBonusType.Evasion, "ab4", 0f, (r, v) => v);
  public static float EvasionBonus() => CalculateBonus(AbilityBonusType.Block, "ab5", 0f, (r, v) => v, false);
  public static float HealBonus() => CalculateBonus(AbilityBonusType.Healing, "ab6", 1f, (r, v) => r + v);
  public static float StunResist() => CalculateBonus(AbilityBonusType.Resist, "ab7", 0f, (r, v) => v, false);
  public static float AttackPriorityBonus() => CalculateBonus(AbilityBonusType.Priority, "ab8", 0f, (r, v) => v, false);
  public static float AmbushProtectBonus() => CalculateBonus(AbilityBonusType.AmbushProtect, "ab9", 0f, (r, v) => v, false);
  public static float XpBonus() => CalculateBonus(AbilityBonusType.Experience, "ab10", 1f, (r, v) => r + v);
  public static float PriceBonus() => CalculateBonus(AbilityBonusType.Prices, "ab11", 1f, (r, v) => r - v);
  public static float ArmorReqBonus() => CalculateBonus(AbilityBonusType.Requirements, "ab12", 0f, (r, v) => v, false);
  public static float CritChanceBonus() => CalculateBonus(AbilityBonusType.Crit, "ab13", 0f, (r, v) => v, false);
  public static float PrecisionBonus() => CalculateBonus(AbilityBonusType.Precision, "ab14", 0f, (r, v) => v, false);
  public static float AllDamageResistBonus() => CalculateBonus(AbilityBonusType.Resist, "ab15", 0f, (r, v) => v, false);
  public static float MovePriorityBonus() => CalculateBonus(AbilityBonusType.Priority, "ab16", 0f, (r, v) => v, false);
  public static float ChargesBonus() => CalculateBonus(AbilityBonusType.Skills, "ab17", 0f, (r, v) => v, false);
  public static float HealthBonus() => CalculateBonus(AbilityBonusType.Health, "ab18", 0f, (r, v) => v, false);
  public static float DamageVsBossesBonus() => CalculateBonus(AbilityBonusType.Damage, "ab19", 1f, (r, v) => r + v);
  public static float TrapsResistBonus() => CalculateBonus(AbilityBonusType.Resist, "ab20", 1f, (r, v) => r - v);
  // TODO: Пока нет атакующих скиллов
  public static float SkillDamageBonus() => CalculateBonus(AbilityBonusType.Skills, "ab21", 1f, (r, v) => r + v);
  // TODO: Пока нет сбития с ног
  public static float KnockdownResistBonus() => CalculateBonus(AbilityBonusType.Resist, "ab22", 0f, (r, v) => v, false);
  public static float CraftPricesBonus() => CalculateBonus(AbilityBonusType.Prices, "ab23", 0f, (r, v) => v, false);
  public static float FameBonus() => CalculateBonus(AbilityBonusType.Fame, "ab24", 1f, (r, v) => r + v);

  private static AbilityInstance[] GetAbilitiesWithBonus(AbilityBonusType bonus) {
    return allAbilities
      .Where(a => a.data.bonusType == bonus && a.level != AbilityLevel.No)
      .ToArray();
  }

  private static int LevelIndex(AbilityLevel level) {
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }
}
