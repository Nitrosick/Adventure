using System.Collections.Generic;
using UnityEngine;

public class MapZoneHub : MonoBehaviour {
  [Header("Features")]
  public bool isHome;
  public MapZoneFeature[] features;
  public List<MapZoneFeature> Upgrades { get; set; } = new() { };
  public Sprite commonAvatar;

  [Header("Healing")]
  public string healerName;
  public MasteryLevel healerLevel;

  [Header("Training")]
  public string trainerName;
  public MasteryLevel trainerLevel;
  public TrainingChain[] soldierTrainingChains;
  public TrainingChain[] additionalSoldierTrainingChains;
  public TrainingChain[] supportTrainingChains;
  public TrainingChain[] additionalSupportTrainingChains;

  [Header("Trading")]
  public string merchantName;
  public MasteryLevel merchantLevel;
  public bool resourcesSale;
  public Equipment[] equipmentGoods;
  public Equipment[] additionalEquipmentGoods;
  public Item[] itemGoods;
  public Item[] additionalItemGoods;

  [Header("Weaponsmith")]
  public string weaponsmithName;
  public MasteryLevel weaponsmithLevel;
  public CraftingRecipe[] weaponsmithRecipes;
  public CraftingRecipe[] weaponsmithAdditionalRecipes;

  [Header("Armorer")]
  public string armorerName;
  public MasteryLevel armorerLevel;
  public CraftingRecipe[] armorerRecipes;
  public CraftingRecipe[] armorerAdditionalRecipes;

  [Header("Quests")]
  public string elderName;
  public MasteryLevel elderLevel;
  public Quest[] quests;

  public void OpenHubMenu() {
    if (features.Length < 1) return;
    HubMenuUI.Open(this);
  }

  public void AddUpgrade(MapZoneFeature feature) {
    if (!Upgrades.Contains(feature)) Upgrades.Add(feature);
    string id = transform.GetComponent<MapZone>().id;
    StateManager.zonesState[id].upgrades = Upgrades.ToArray();
  }
}
