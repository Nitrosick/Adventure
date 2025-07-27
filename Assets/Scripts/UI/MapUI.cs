using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {
  // Zone info
  private static GameObject zoneInfoPanel;
  private static TextMeshProUGUI zoneInfoTitle;
  private static TextMeshProUGUI zoneInfoDescription;
  private static GameObject zoneInfoBattleMark;
  private static GameObject zoneInfoGuardedMark;
  private static GameObject zoneInfoClearedMark;
  private static GameObject zoneInfoRecruitMark;

  // Buttons
  private static Button mainMenuButton;
  private static Button playerMenuButton;
  private static Button interactButton;

  // Resources
  private static TextMeshProUGUI goldValue;
  private static TextMeshProUGUI woodValue;
  private static TextMeshProUGUI stoneValue;
  private static TextMeshProUGUI metalValue;
  private static TextMeshProUGUI leatherValue;
  private static TextMeshProUGUI villagersValue;

  private T Get<T>(Transform parent, string path) where T : Component {
    return parent.Find(path).GetComponent<T>();
  }

  private GameObject Find(Transform parent, string path) {
    return parent.Find(path).gameObject;
  }

  private void Awake() {
    Transform infoPanel = transform.Find("Info/ZoneInfoPanel");
    Transform markers = infoPanel.Find("Markers");
    Transform top = transform.Find("Top");
    Transform mainMenu = top.Find("MainMenu");
    Transform resources = top.Find("Resources");
    Transform actions = transform.Find("Actions");

    zoneInfoPanel = infoPanel.gameObject;
    zoneInfoTitle = Get<TextMeshProUGUI>(infoPanel, "Title");
    zoneInfoDescription = Get<TextMeshProUGUI>(infoPanel, "Description");

    zoneInfoBattleMark = Find(markers, "Battle");
    zoneInfoGuardedMark = Find(markers, "Guard");
    zoneInfoClearedMark = Find(markers, "Clear");
    zoneInfoRecruitMark = Find(markers, "Recruitment");

    mainMenuButton = Get<Button>(mainMenu, "Main");
    playerMenuButton = Get<Button>(mainMenu, "Player");
    interactButton = Get<Button>(actions, "Interact");

    goldValue = Get<TextMeshProUGUI>(resources, "Gold/Value");
    woodValue = Get<TextMeshProUGUI>(resources, "Wood/Value");
    stoneValue = Get<TextMeshProUGUI>(resources, "Stone/Value");
    metalValue = Get<TextMeshProUGUI>(resources, "Metal/Value");
    leatherValue = Get<TextMeshProUGUI>(resources, "Leather/Value");
    villagersValue = Get<TextMeshProUGUI>(resources, "Villagers/Value");

    if (!ComponentsInitialized()) {
      Debug.LogError("Map UI components initialization error");
    }

    mainMenuButton.onClick.AddListener(OpenPauseMenu);
    playerMenuButton.onClick.AddListener(SwitchPlayerMenu);
    EnableUI();
  }

  private static bool ComponentsInitialized() {
    return zoneInfoPanel != null && zoneInfoTitle != null && zoneInfoDescription != null &&
      zoneInfoGuardedMark != null && mainMenuButton != null && playerMenuButton != null &&
      goldValue != null && woodValue != null && stoneValue != null &&
      metalValue != null && villagersValue != null && leatherValue != null &&
      zoneInfoBattleMark != null && zoneInfoClearedMark != null && zoneInfoRecruitMark != null &&
      interactButton != null;
  }

  private void OnDestroy() {
    mainMenuButton.onClick.RemoveListener(OpenPauseMenu);
    playerMenuButton.onClick.RemoveListener(SwitchPlayerMenu);
  }

  public static void DisableUI() {
    mainMenuButton.interactable = false;
    playerMenuButton.interactable = false;
  }

  public static void EnableUI() {
    mainMenuButton.interactable = true;
    playerMenuButton.interactable = true;
  }

  public static void ShowZoneInfo(string title, string desc, string descCleared, List<MapZoneType> events, bool empty) {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(true);
    zoneInfoTitle.text = title;

    if (empty) {
      zoneInfoDescription.text = desc;
      return;
    }

    zoneInfoDescription.text = events.Count == 0 ? descCleared : desc;
    if (events.Count == 0) zoneInfoClearedMark.SetActive(true);
    else if (events.Contains(MapZoneType.InstantBattle)) zoneInfoBattleMark.SetActive(true);
    else if (events.Contains(MapZoneType.Guard)) zoneInfoGuardedMark.SetActive(true);
    else if (events.Contains(MapZoneType.Recruitment)) zoneInfoRecruitMark.SetActive(true);
  }

  public static void HideZoneInfo() {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(false);
    zoneInfoTitle.text = "";
    zoneInfoDescription.text = "";
    zoneInfoBattleMark.SetActive(false);
    zoneInfoGuardedMark.SetActive(false);
    zoneInfoClearedMark.SetActive(false);
    zoneInfoRecruitMark.SetActive(false);
  }

  public static void ShowInteractableButton(UnityAction callback) {
    // FIXME: Смена текста и иконки
    interactButton.onClick.AddListener(callback);
    interactButton.gameObject.SetActive(true);
  }

  public static void HideInteractableButton() {
    interactButton.gameObject.SetActive(false);
    interactButton.onClick.RemoveAllListeners();
  }

  public static void UpdateResources() {
    Player player = Player.Instance;

    goldValue.text = player.Gold.ToString();
    woodValue.text = player.Resources[0].ToString();
    stoneValue.text = player.Resources[1].ToString();
    metalValue.text = player.Resources[2].ToString();
    leatherValue.text = player.Resources[3].ToString();

    int[] totalPeople = player.GetTotalPeople();
    villagersValue.text = string.Format(
      "{0} ({1}) / {2}",
      totalPeople[0].ToString(),
      totalPeople[1].ToString(),
      player.MaxVillagers.ToString()
    );
    if (totalPeople[0] + totalPeople[1] > player.MaxVillagers) {
      villagersValue.text = "<color=#F61010>" + villagersValue.text + "</color>";
    }
  }

  private static void OpenPauseMenu() {
    CloseOtherWindows();
    PlayerMenuUI.Close();
    PauseMenu.Open();
  }

  private static void SwitchPlayerMenu() {
    CloseOtherWindows();
    PlayerMenuUI.Switch();
  }

  private static void CloseOtherWindows() {
    RecruitingUI.Close();
    HomeMenuUI.Close();
  }
}
