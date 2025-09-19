using System;
using System.Linq;

[Serializable]
public class CraftingRecipe {
  public Equipment sourceEquip;

  public Item[] componentItems;
  // FIXME: Стакать, если несколько одинаковых
  public int[] componentResources = { 0, 0, 0, 0 };
  public int cost;

  public Equipment resultEquip;
  public Item resultItem;
  public int resultCount = 1;

  public int[] GetComponentResources() {
    return componentResources
      .Select(r => r == 0 ? 0 : Math.Max(1, r - (int)AbilityController.CraftPricesBonus()))
      .ToArray();
  }

  public int GetCost() {
    return Math.Max(1, cost - (int)AbilityController.CraftPricesBonus() * 10);
  }
}
