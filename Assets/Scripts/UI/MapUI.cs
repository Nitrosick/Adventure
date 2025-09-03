using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapUI : GeneralUI {
  public static MapUI Instance;
  private static IconDatabase IconDatabase;
  public GameObject emptySlotPrefab;
  public Sprite villagersSprite;
  public Sprite[] resourceSprites;

  // Zone info
  private GameObject zoneInfoPanel;
  private TextMeshProUGUI zoneInfoTitle;
  private TextMeshProUGUI zoneInfoDescription;
  private GameObject zoneInfoBattleMark;
  private GameObject zoneInfoClearedMark;
  private GameObject zoneInfoRecruitMark;
  private GameObject zoneInfoQuestMark;

  // Buttons
  private Button playerMenuButton;
  private Button interactButton;
  private Image interactButtonIcon;
  private TextMeshProUGUI interactButtonText;

  // Resources
  private TextMeshProUGUI goldValue;
  private TextMeshProUGUI woodValue;
  private TextMeshProUGUI stoneValue;
  private TextMeshProUGUI metalValue;
  private TextMeshProUGUI leatherValue;
  private TextMeshProUGUI villagersValue;
  private TextMeshProUGUI location;
  public string[] resTooltips = { "Wood", "Stone", "Metal", "Leather" };

  protected override void Awake() {
    base.Awake();
    Instance = this;
    IconDatabase = Resources.Load<IconDatabase>("Databases/IconDatabase");

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
    zoneInfoClearedMark = Find(markers, "Clear");
    zoneInfoRecruitMark = Find(markers, "Recruitment");
    zoneInfoQuestMark = Find(markers, "Quest");

    playerMenuButton = Get<Button>(mainMenu, "Player");
    interactButton = Get<Button>(actions, "Interact");
    interactButtonIcon = Get<Image>(actions, "Interact/Icon");
    interactButtonText = Get<TextMeshProUGUI>(actions, "Interact/Text");

    goldValue = Get<TextMeshProUGUI>(resources, "Gold/Value");
    woodValue = Get<TextMeshProUGUI>(resources, "Wood/Value");
    stoneValue = Get<TextMeshProUGUI>(resources, "Stone/Value");
    metalValue = Get<TextMeshProUGUI>(resources, "Metal/Value");
    leatherValue = Get<TextMeshProUGUI>(resources, "Leather/Value");
    villagersValue = Get<TextMeshProUGUI>(resources, "Villagers/Value");
    location = Get<TextMeshProUGUI>(resources, "Location");

    if (!ComponentsInitialized()) {
      Debug.LogError("Map UI components initialization error");
    }

    playerMenuButton.onClick.AddListener(SwitchPlayerMenu);
    EnableUI();
  }

  private bool ComponentsInitialized() {
    return zoneInfoPanel != null && zoneInfoTitle != null && zoneInfoDescription != null &&
      zoneInfoQuestMark != null && playerMenuButton != null && goldValue != null &&
      woodValue != null && stoneValue != null && metalValue != null &&
      villagersValue != null && leatherValue != null && zoneInfoBattleMark != null &&
      zoneInfoClearedMark != null && zoneInfoRecruitMark != null && interactButton != null &&
      location != null && interactButtonIcon != null && interactButtonText != null;
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    playerMenuButton.onClick.RemoveListener(SwitchPlayerMenu);
  }

  public override void DisableUI() {
    base.DisableUI();
    playerMenuButton.interactable = false;
  }

  public override void EnableUI() {
    base.EnableUI();
    playerMenuButton.interactable = true;
  }

  protected override void OpenPauseMenu() {
    CloseOtherWindows();
    PlayerMenuUI.Close();
    AlmanacUI.Close();
    PauseMenu.Open();
  }

  private void SwitchPlayerMenu() {
    CloseOtherWindows();
    AlmanacUI.Close();
    PlayerMenuUI.Switch();
  }

  protected override void OpenAlmanac() {
    CloseOtherWindows();
    PlayerMenuUI.Close();
    AlmanacUI.Open();
  }

  private void CloseOtherWindows() {
    RecruitingUI.Close();
    HomeMenuUI.Close();
  }

  public void ShowZoneInfo(string title, string desc, string descCleared, List<MapZoneType> events, bool empty) {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(true);
    zoneInfoTitle.text = title;

    if (empty) {
      zoneInfoDescription.text = desc;
      return;
    }

    zoneInfoDescription.text = events.Count == 0 ? descCleared : desc;
    if (events.Count == 0) zoneInfoClearedMark.SetActive(true);
    else if (events.Contains(MapZoneType.Battle)) zoneInfoBattleMark.SetActive(true);
    else if (events.Contains(MapZoneType.Recruitment)) zoneInfoRecruitMark.SetActive(true);
    else if (events.Contains(MapZoneType.Quest)) zoneInfoQuestMark.SetActive(true);
  }

  public void HideZoneInfo() {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(false);
    zoneInfoTitle.text = "";
    zoneInfoDescription.text = "";
    zoneInfoBattleMark.SetActive(false);
    zoneInfoClearedMark.SetActive(false);
    zoneInfoRecruitMark.SetActive(false);
    zoneInfoQuestMark.SetActive(false);
  }

  public void ShowInteractableButton(UnityAction callback, string icon = "settings", string text = "Interact") {
    Sprite sprite = IconDatabase.GetIcon(icon);
    interactButtonIcon.sprite = sprite;
    interactButtonText.text = text;
    interactButton.onClick.AddListener(callback);
    interactButton.gameObject.SetActive(true);
  }

  public void HideInteractableButton() {
    interactButton.gameObject.SetActive(false);
    interactButton.onClick.RemoveAllListeners();
  }

  public void UpdateResources() {
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

  public void UpdateLocation(string value) {
    location.text = value;
  }
}
