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
}
