using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
  // Zone info
  private static GameObject zoneInfoPanel;
  private static TextMeshProUGUI zoneInfoTitle;
  private static TextMeshProUGUI zoneInfoDescription;
  private static GameObject zoneInfoGuardedMark;

  // Buttons
  private static Button mainMenuButton;
  private static Button playerMenuButton;

  // Resources
  private static TextMeshProUGUI goldValue;
  private static TextMeshProUGUI woodValue;
  private static TextMeshProUGUI stoneValue;
  private static TextMeshProUGUI metalsValue;
  private static TextMeshProUGUI villagersValue;

  private void Awake() {
    zoneInfoPanel = transform.Find("Info/ZoneInfoPanel").gameObject;
    zoneInfoTitle = transform.Find("Info/ZoneInfoPanel/Title").GetComponent<TextMeshProUGUI>();
    zoneInfoDescription = transform.Find("Info/ZoneInfoPanel/Description").GetComponent<TextMeshProUGUI>();
    zoneInfoGuardedMark = transform.Find("Info/ZoneInfoPanel/GuardMarker").gameObject;

    mainMenuButton = transform.Find("Top/MainMenu/Main").GetComponent<Button>();
    playerMenuButton = transform.Find("Top/MainMenu/Player").GetComponent<Button>();

    Transform resources = transform.Find("Top/Resources").GetComponent<Transform>();
    goldValue = resources.Find("Gold/Value").GetComponent<TextMeshProUGUI>();
    woodValue = resources.Find("Wood/Value").GetComponent<TextMeshProUGUI>();
    stoneValue = resources.Find("Stone/Value").GetComponent<TextMeshProUGUI>();
    metalsValue = resources.Find("Metals/Value").GetComponent<TextMeshProUGUI>();
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
    metalsValue != null && villagersValue != null;
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

  public static void ShowZoneInfo(string title, bool cleared, string desc, string descCleared, bool guarded) {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(true);
    zoneInfoTitle.text = title;
    zoneInfoDescription.text = cleared ? descCleared : desc;
    if (guarded) zoneInfoGuardedMark.SetActive(true);
  }

  public static void HideZoneInfo() {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(false);
    zoneInfoTitle.text = "";
    zoneInfoDescription.text = "";
    zoneInfoGuardedMark.SetActive(false);
  }

  public static void UpdateResources(int gold, int[] resources, int[] totalPeople, int max) {
    goldValue.text = gold.ToString();
    woodValue.text = resources[0].ToString();
    stoneValue.text = resources[1].ToString();
    metalsValue.text = resources[2].ToString();

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
