using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utils {
  private static readonly string[] c = {
    "#A0A0A0",
    "#618C2D",
    "#306DAB",
    "#6948A4",
    "#CF8F0B",
    "#A7E7E4"
  };

  public static bool RollChance(float chance) {
    return UnityEngine.Random.Range(0f, 100f) < chance;
  }

  public static int GetRandomInRange(int min, int max) {
    return UnityEngine.Random.Range(min, max + 1);
  }

  private static Dictionary<TEnum, Color> CreatePalette<TEnum>(Dictionary<TEnum, string> hexMap) where TEnum : Enum {
    var palette = new Dictionary<TEnum, Color>();
    foreach (var kvp in hexMap)
      if (ColorUtility.TryParseHtmlString(kvp.Value, out var color))
        palette[kvp.Key] = color;

    return palette;
  }

  public static Dictionary<Rarity, Color> GetRarityPalette() =>
    CreatePalette(new Dictionary<Rarity, string>
    {
      { Rarity.Common, c[0] },
      { Rarity.Rare, c[1] },
      { Rarity.Epic, c[2] },
      { Rarity.Legendary, c[3] },
      { Rarity.Relic, c[4] }
    });

  public static Dictionary<MasteryLevel, Color> GetMasteryPalette() =>
    CreatePalette(new Dictionary<MasteryLevel, string>
    {
      { MasteryLevel.Novice, c[0] },
      { MasteryLevel.Apprentice, c[1] },
      { MasteryLevel.Adept, c[2] },
      { MasteryLevel.Expert, c[3] },
      { MasteryLevel.Master, c[4] }
    });

  public static Dictionary<AbilityLevel, Color> GetAbilityLevelPalette() =>
    CreatePalette(new Dictionary<AbilityLevel, string>
    {
      { AbilityLevel.No, "#FFFFFF" },
      { AbilityLevel.Bronze, "#C2771D" },
      { AbilityLevel.Silver, "#CDCDCE" },
      { AbilityLevel.Gold, "#E2B63F" }
    });

  public static string SplitPascalCase(string input) {
    return Regex.Replace(input, "(?<!^)([A-Z])", " $1").Trim();
  }
}
