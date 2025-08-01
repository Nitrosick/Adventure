using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenuUI : MonoBehaviour {
  // Components
  private static Transform menu;
  private static Transform content;
  private static MapZoneHome mapZone;

  // Navigation
  private static Transform navigation;
  private static Button healingFeature;
  private static Button tradingFeature;
  private static Button weaponUpFeature;
  private static Button armorUpFeature;
  private static Button saveButton;
  private static Button closeButton;

  //Sections
  private static Transform welcomeSection;
  private static HealingMenuUI healingSection;
  private static TradingMenuUI tradingSection;
  private static CraftingMenuUI craftingSection;

  private static readonly int saveDelay = 3;

  private void Awake() {
    menu = transform.Find("HomeMenu/Panel");
    content = menu.Find("Content/Viewport");

    static Transform Find(string path) => menu.Find(path);
    static Transform FindInContent(string path) => content.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();
    T GetInContent<T>(string path) where T : Component => content.Find(path).GetComponent<T>();

    navigation = Find("Navigation/Features");
    healingFeature = Get<Button>("Navigation/Features/Healing");
    tradingFeature = Get<Button>("Navigation/Features/Trading");
    weaponUpFeature = Get<Button>("Navigation/Features/WeaponUp");
    armorUpFeature = Get<Button>("Navigation/Features/ArmorUp");
    saveButton = Get<Button>("Navigation/Control/Save");
    closeButton = Get<Button>("Navigation/Control/Close");

    welcomeSection = FindInContent("Scroll/Welcome");
    healingSection = GetInContent<HealingMenuUI>("Scroll/Healing");
    tradingSection = GetInContent<TradingMenuUI>("Scroll/Trading");
    craftingSection = GetInContent<CraftingMenuUI>("Scroll/Crafting");

    if (!ComponentsInitialized()) {
      Debug.LogError("Home menu UI components initialization error");
      return;
    }

    healingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Healing));
    tradingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Trading));
    weaponUpFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Weaponsmith));
    armorUpFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Armorer));
    saveButton.onClick.AddListener(SaveGame);
    closeButton.onClick.AddListener(Close);
  }

  private static bool ComponentsInitialized() {
    return menu != null && navigation != null && closeButton != null &&
    saveButton != null && healingFeature != null && healingSection != null &&
    welcomeSection != null && tradingFeature != null && tradingSection != null &&
    weaponUpFeature != null && armorUpFeature != null && craftingSection != null;
  }

  private void OnDestroy() {
    healingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Healing));
    tradingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Trading));
    weaponUpFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Weaponsmith));
    armorUpFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Armorer));
    saveButton.onClick.RemoveListener(SaveGame);
    closeButton.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneHome zone) {
    mapZone = zone;
    EnableButtons(zone.features);
    menu.gameObject.SetActive(true);
    SceneController.OpenWindow("home");
  }

  public static void Close() {
    menu.gameObject.SetActive(false);
    mapZone = null;
    DisableButtons();
    HideSections();
    welcomeSection.gameObject.SetActive(true);
    SceneController.CloseWindow("home");
  }

  private static void EnableButtons(MapZoneFeature[] features) {
    if (features.Contains(MapZoneFeature.Healing)) healingFeature.interactable = true;
    if (features.Contains(MapZoneFeature.Trading)) tradingFeature.interactable = true;
    if (features.Contains(MapZoneFeature.Weaponsmith) && mapZone.weaponsmithRecipes.Length > 0) weaponUpFeature.interactable = true;
    if (features.Contains(MapZoneFeature.Armorer) && mapZone.armorerRecipes.Length > 0) armorUpFeature.interactable = true;
  }

  private static void DisableButtons() {
    healingFeature.interactable = false;
    tradingFeature.interactable = false;
    weaponUpFeature.interactable = false;
    armorUpFeature.interactable = false;
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
        healingSection.Init(mapZone.healerName, mapZone.healerLevel);
        break;
      case MapZoneFeature.Trading:
        tradingSection.gameObject.SetActive(true);
        tradingSection.Init(
          mapZone.merchantName,
          mapZone.merchantLevel,
          mapZone.resourcesSale,
          mapZone.equipmentGoods,
          mapZone.itemGoods
        );
        break;
      case MapZoneFeature.Weaponsmith:
        craftingSection.gameObject.SetActive(true);
        craftingSection.Init(
          mapZone.weaponsmithName,
          mapZone.weaponsmithLevel,
          feature,
          mapZone.weaponsmithRecipes
        );
        break;
      case MapZoneFeature.Armorer:
        craftingSection.gameObject.SetActive(true);
        craftingSection.Init(
          mapZone.armorerName,
          mapZone.armorerLevel,
          feature,
          mapZone.armorerRecipes
        );
        break;
    }
  }

  private static void HideSections() {
    welcomeSection.gameObject.SetActive(false);
    healingSection.Clear();
    tradingSection.Clear();
    craftingSection.Clear();
  }

  public static void RecalculateRecipes() {
    foreach (CraftingRecipeUI recipe in content.GetComponentsInChildren<CraftingRecipeUI>()) {
      recipe.CheckEnoughResources();
    }
  }
}
