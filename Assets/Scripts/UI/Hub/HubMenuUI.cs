using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HubMenuUI : MonoBehaviour {
  // Components
  private static Transform menu;
  private static Transform content;
  private static MapZoneHub mapZone;

  // Navigation
  private static Transform navigation;
  private static Button healingFeature;
  private static Button trainingFeature;
  private static Button tradingFeature;
  private static Button weaponUpFeature;
  private static Button armorUpFeature;
  private static Button questsFeature;
  private static Button saveButton;
  private static Button closeButton;
  private static Dictionary<MapZoneFeature, Button> featureButtons = new() { };

  //Sections
  private static Transform welcomeSection;
  private static HealingMenuUI healingSection;
  private static TrainingMenuUI trainingSection;
  private static TradingMenuUI tradingSection;
  private static CraftingMenuUI craftingSection;
  private static QuestsMenuUI questsSection;

  private static readonly int saveDelay = 3;

  void Awake() {
    menu = transform.Find("Menu/Hub");
    content = menu.Find("Content/Viewport/Scroll");

    static Transform Find(string path) => menu.Find(path);
    static Transform FindInContent(string path) => content.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();
    T GetInContent<T>(string path) where T : Component => content.Find(path).GetComponent<T>();

    navigation = Find("Navigation/Features");
    healingFeature = Get<Button>("Navigation/Features/Healing");
    trainingFeature = Get<Button>("Navigation/Features/Training");
    tradingFeature = Get<Button>("Navigation/Features/Trading");
    weaponUpFeature = Get<Button>("Navigation/Features/WeaponUp");
    armorUpFeature = Get<Button>("Navigation/Features/ArmorUp");
    questsFeature = Get<Button>("Navigation/Features/Quests");
    saveButton = Get<Button>("Navigation/Control/Save");
    closeButton = Get<Button>("Navigation/Control/Close");

    welcomeSection = FindInContent("Welcome");
    healingSection = GetInContent<HealingMenuUI>("Healing");
    trainingSection = GetInContent<TrainingMenuUI>("Training");
    tradingSection = GetInContent<TradingMenuUI>("Trading");
    craftingSection = GetInContent<CraftingMenuUI>("Crafting");
    questsSection = GetInContent<QuestsMenuUI>("Quests");

    if (!ComponentsInitialized()) {
      Debug.LogError("Hub menu UI components initialization error");
      return;
    }

    featureButtons = new Dictionary<MapZoneFeature, Button>() {
      { MapZoneFeature.Healing, healingFeature },
      { MapZoneFeature.Training, trainingFeature },
      { MapZoneFeature.Trading, tradingFeature },
      { MapZoneFeature.Weaponsmith, weaponUpFeature },
      { MapZoneFeature.Armorer, armorUpFeature },
      { MapZoneFeature.Quests, questsFeature }
    };

    healingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Healing));
    trainingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Training));
    tradingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Trading));
    weaponUpFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Weaponsmith));
    armorUpFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Armorer));
    questsFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Quests));
    saveButton.onClick.AddListener(SaveGame);
    closeButton.onClick.AddListener(Close);
  }

  private static bool ComponentsInitialized() {
    return new object[] {
      menu, navigation, closeButton, saveButton, healingFeature,
      healingSection, welcomeSection, tradingFeature, tradingSection, weaponUpFeature,
      armorUpFeature, craftingSection, trainingFeature, trainingSection, questsFeature,
      questsSection
    }.All(x => x != null);
  }

  void OnDestroy() {
    healingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Healing));
    trainingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Training));
    tradingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Trading));
    weaponUpFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Weaponsmith));
    armorUpFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Armorer));
    questsFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Quests));
    saveButton.onClick.RemoveListener(SaveGame);
    closeButton.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneHub zone) {
    mapZone = zone;
    EnableButtons(zone.features);
    saveButton.gameObject.SetActive(zone.isHome);
    menu.gameObject.SetActive(true);
    SceneController.OpenWindow("hub");
  }

  public static void Close() {
    menu.gameObject.SetActive(false);
    mapZone = null;
    DisableButtons();
    HideSections();
    welcomeSection.gameObject.SetActive(true);
    SceneController.CloseWindow("hub");
  }

  private static void EnableButtons(MapZoneFeature[] features) {
    foreach (var feature in features) {
      if (featureButtons.TryGetValue(feature, out var button)) {
        button.interactable = true;
      }
    }
  }

  private static void DisableButtons() {
    foreach (var button in featureButtons.Values) {
      button.interactable = false;
    }
  }

  private static async void SaveGame() {
    StateManager.SaveGame();
    _ = Toast.Show("success", "Game saved");
    saveButton.interactable = false;
    await Task.Delay(saveDelay * 1000);
    saveButton.interactable = true;
  }

  private static void OpenSection(MapZoneFeature feature) {
    HideSections();

    switch (feature) {
      case MapZoneFeature.Healing:
        healingSection.gameObject.SetActive(true);
        healingSection.Init(
          mapZone.healerName,
          mapZone.healerLevel,
          mapZone.commonAvatar
        );
        break;
      case MapZoneFeature.Training:
        trainingSection.gameObject.SetActive(true);

        List<TrainingChain> soldierTrainingChains = mapZone.soldierTrainingChains.ToList();
        List<TrainingChain> supportTrainingChains = mapZone.supportTrainingChains.ToList();

        if (mapZone.Upgrades.Contains(MapZoneFeature.Training)) {
          soldierTrainingChains.AddRange(mapZone.additionalSoldierTrainingChains);
          supportTrainingChains.AddRange(mapZone.additionalSupportTrainingChains);
        }          

        trainingSection.Init(
          mapZone.trainerName,
          mapZone.trainerLevel,
          soldierTrainingChains,
          supportTrainingChains,
          mapZone.commonAvatar
        );
        break;
      case MapZoneFeature.Trading:
        tradingSection.gameObject.SetActive(true);

        List<Equipment> equipmentGoods = mapZone.equipmentGoods.ToList();
        List<Item> itemGoods = mapZone.itemGoods.ToList();

        if (mapZone.Upgrades.Contains(MapZoneFeature.Trading)) {
          equipmentGoods.AddRange(mapZone.additionalEquipmentGoods);
          itemGoods.AddRange(mapZone.additionalItemGoods);
        }          

        tradingSection.Init(
          mapZone.merchantName,
          mapZone.merchantLevel,
          mapZone.resourcesSale,
          equipmentGoods,
          itemGoods,
          mapZone.commonAvatar
        );
        break;
      case MapZoneFeature.Weaponsmith:
        craftingSection.gameObject.SetActive(true);

        List<CraftingRecipe> weaponRecipes = mapZone.weaponsmithRecipes.ToList();
        if (mapZone.Upgrades.Contains(MapZoneFeature.Weaponsmith))
          weaponRecipes.AddRange(mapZone.weaponsmithAdditionalRecipes);

        craftingSection.Init(
          mapZone.weaponsmithName,
          mapZone.weaponsmithLevel,
          feature,
          weaponRecipes,
          mapZone.commonAvatar
        );
        break;
      case MapZoneFeature.Armorer:
        craftingSection.gameObject.SetActive(true);

        List<CraftingRecipe> armorRecipes = mapZone.armorerRecipes.ToList();
        if (mapZone.Upgrades.Contains(MapZoneFeature.Armorer))
          armorRecipes.AddRange(mapZone.armorerAdditionalRecipes);

        craftingSection.Init(
          mapZone.armorerName,
          mapZone.armorerLevel,
          feature,
          armorRecipes,
          mapZone.commonAvatar
        );
        break;
      case MapZoneFeature.Quests:
        questsSection.gameObject.SetActive(true);
        questsSection.Init(
          mapZone.elderName,
          mapZone.elderLevel,
          mapZone.quests,
          mapZone.commonAvatar
        );
        break;
    }
  }

  private static void HideSections() {
    welcomeSection.gameObject.SetActive(false);
    healingSection.Clear();
    trainingSection.Clear();
    tradingSection.Clear();
    craftingSection.Clear();
    questsSection.Clear();
  }

  public static void RecalculateRecipes() {
    foreach (CraftingRecipeUI recipe in content.GetComponentsInChildren<CraftingRecipeUI>()) {
      recipe.CheckEnoughResources();
    }
    foreach (TrainingChainUI chain in content.GetComponentsInChildren<TrainingChainUI>()) {
      chain.CheckEnoughResources();
    }
  }
}
