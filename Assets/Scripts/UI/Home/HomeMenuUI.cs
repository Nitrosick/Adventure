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
  private static Button saveButton;
  private static Button closeButton;

  //Sections
  private static Transform welcomeSection;
  private static HealingMenuUI healingSection;
  private static TradingMenuUI tradingSection;

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
    saveButton = Get<Button>("Navigation/Control/Save");
    closeButton = Get<Button>("Navigation/Control/Close");

    welcomeSection = FindInContent("Scroll/Welcome");
    healingSection = GetInContent<HealingMenuUI>("Scroll/Healing");
    tradingSection = GetInContent<TradingMenuUI>("Scroll/Trading");

    if (!ComponentsInitialized()) {
      Debug.LogError("Home menu UI components initialization error");
      return;
    }

    healingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Healing));
    tradingFeature.onClick.AddListener(() => OpenSection(MapZoneFeature.Trading));
    saveButton.onClick.AddListener(SaveGame);
    closeButton.onClick.AddListener(Close);
  }

  private static bool ComponentsInitialized() {
    return menu != null && navigation != null && closeButton != null &&
    saveButton != null && healingFeature != null && healingSection != null &&
    welcomeSection != null && tradingFeature != null && tradingSection != null;
  }

  private void OnDestroy() {
    healingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Healing));
    tradingFeature.onClick.RemoveListener(() => OpenSection(MapZoneFeature.Trading));
    saveButton.onClick.RemoveListener(SaveGame);
    closeButton.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneHome zone) {
    mapZone = zone;
    EnableButtons(zone.features);
    menu.gameObject.SetActive(true);
    SceneController.ShowBackground();
  }

  private static void Close() {
    menu.gameObject.SetActive(false);
    SceneController.HideBackground();
    mapZone = null;
    DisableButtons();
    HideSections();
    welcomeSection.gameObject.SetActive(true);
  }

  private static void EnableButtons(MapZoneFeature[] features) {
    if (features.Contains(MapZoneFeature.Healing)) healingFeature.interactable = true;
    if (features.Contains(MapZoneFeature.Trading)) tradingFeature.interactable = true;
  }

  private static void DisableButtons() {
    healingFeature.interactable = false;
    tradingFeature.interactable = false;
  }

  private static async void SaveGame() {
    StateManager.SaveGame();
    _ = InfoPopup.Show("success", "Game saved");
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
    }
  }

  private static void HideSections() {
    welcomeSection.gameObject.SetActive(false);
    healingSection.Clear();
    tradingSection.Clear();
  }
}
