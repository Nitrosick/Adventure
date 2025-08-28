using UnityEngine;

public class MapZoneHome : MonoBehaviour {
  [Header("Features")]
  public MapZoneFeature[] features;

  [Header("Healing")]
  public string healerName;
  public MasteryLevel healerLevel;

  [Header("Training")]
  public string trainerName;
  public MasteryLevel trainerLevel;
  public TrainingChain[] soldierTrainingChains;
  public TrainingChain[] supportTrainingChains;

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

  [Header("Quests")]
  public string elderName;
  public MasteryLevel elderLevel;
  public Quest[] quests;

  public void OpenHomeMenu() {
    if (features.Length < 1) return;
    HomeMenuUI.Open(this);
  }
}
