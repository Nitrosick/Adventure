using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
  // Zone info
  private static GameObject zoneInfoPanel;
  private static TextMeshProUGUI zoneInfoTitle;
  private static TextMeshProUGUI zoneInfoDescription;
  private static GameObject zoneInfoBattleMark;
  private static GameObject zoneInfoGuardedMark;
  private static GameObject zoneInfoClearedMark;

  // Buttons
  private static Button mainMenuButton;
  private static Button playerMenuButton;

  // Resources
  private static TextMeshProUGUI goldValue;
  private static TextMeshProUGUI woodValue;
  private static TextMeshProUGUI stoneValue;
  private static TextMeshProUGUI metalValue;
  private static TextMeshProUGUI leatherValue;
  private static TextMeshProUGUI villagersValue;

  private void Awake() {
    zoneInfoPanel = transform.Find("Info/ZoneInfoPanel").gameObject;
    zoneInfoTitle = transform.Find("Info/ZoneInfoPanel/Title").GetComponent<TextMeshProUGUI>();
    zoneInfoDescription = transform.Find("Info/ZoneInfoPanel/Description").GetComponent<TextMeshProUGUI>();
    zoneInfoBattleMark = transform.Find("Info/ZoneInfoPanel/Markers/Battle").gameObject;
    zoneInfoGuardedMark = transform.Find("Info/ZoneInfoPanel/Markers/Guard").gameObject;
    zoneInfoClearedMark = transform.Find("Info/ZoneInfoPanel/Markers/Clear").gameObject;

    mainMenuButton = transform.Find("Top/MainMenu/Main").GetComponent<Button>();
    playerMenuButton = transform.Find("Top/MainMenu/Player").GetComponent<Button>();

    Transform resources = transform.Find("Top/Resources").GetComponent<Transform>();
    goldValue = resources.Find("Gold/Value").GetComponent<TextMeshProUGUI>();
    woodValue = resources.Find("Wood/Value").GetComponent<TextMeshProUGUI>();
    stoneValue = resources.Find("Stone/Value").GetComponent<TextMeshProUGUI>();
    metalValue = resources.Find("Metal/Value").GetComponent<TextMeshProUGUI>();
    leatherValue = resources.Find("Leather/Value").GetComponent<TextMeshProUGUI>();
    villagersValue = resources.Find("Villagers/Value").GetComponent<TextMeshProUGUI>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Map UI components initialization error");
    }

    playerMenuButton.onClick.AddListener(() => PlayerMenuUI.Switch());
    EnableUI();
  }

  private static bool ComponentsInitialized() {
    return zoneInfoPanel != null && zoneInfoTitle != null && zoneInfoDescription != null &&
      zoneInfoGuardedMark != null && mainMenuButton != null && playerMenuButton != null &&
      goldValue != null && woodValue != null && stoneValue != null &&
      metalValue != null && villagersValue != null && leatherValue != null &&
      zoneInfoBattleMark != null && zoneInfoClearedMark != null;
  }

  private void OnDestroy() {
    playerMenuButton.onClick.RemoveListener(() => PlayerMenuUI.Switch());
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
  }

  public static void HideZoneInfo() {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(false);
    zoneInfoTitle.text = "";
    zoneInfoDescription.text = "";
    zoneInfoBattleMark.SetActive(false);
    zoneInfoGuardedMark.SetActive(false);
    zoneInfoClearedMark.SetActive(false);
  }

  public static void UpdateResources(int gold, int[] resources, int[] totalPeople, int max) {
    goldValue.text = gold.ToString();
    woodValue.text = resources[0].ToString();
    stoneValue.text = resources[1].ToString();
    metalValue.text = resources[2].ToString();
    leatherValue.text = resources[3].ToString();

    villagersValue.text = string.Format(
      "{0} ({1}) / {2}",
      totalPeople[0].ToString(),
      totalPeople[1].ToString(),
      max.ToString()
    );
    if (totalPeople[0] + totalPeople[1] > max) {
      villagersValue.text = "<color=#F61010>" + villagersValue.text + "</color>";
    }
  }
}
