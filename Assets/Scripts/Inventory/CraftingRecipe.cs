[System.Serializable]
public class CraftingRecipe {
  public Equipment sourceEquip;

  public Item[] componentItems;
  // FIXME: Стакать, если несколько одинаковых
  public int[] componentResources = { 0, 0, 0, 0 };
  public int cost;

  public Equipment resultEquip;
  public Item resultItem;
  public int resultCount = 1;
}
