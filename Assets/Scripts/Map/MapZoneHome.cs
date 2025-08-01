using UnityEngine;

public class MapZoneHome : MapZone {
  [Header("Features")]
  public MapZoneFeature[] features;

  [Header("Healing")]
  public string healerName;
  public MasteryLevel healerLevel;

  [Header("Trading")]
  public string merchantName;
  public MasteryLevel merchantLevel;
  public bool resourcesSale;
  public Equipment[] equipmentGoods;
  public Item[] itemGoods;

  [Header("Weaponsmith")]
  public string weaponsmithName;
  public MasteryLevel weaponsmithLevel;
  public CraftingRecipe[] weaponsmithRecipes;

  [Header("Armorer")]
  public string armorerName;
  public MasteryLevel armorerLevel;
  public CraftingRecipe[] armorerRecipes;

  public void OpenHomeMenu() {
    if (features.Length < 1) return;
    HomeMenuUI.Open(this);
  }
}
