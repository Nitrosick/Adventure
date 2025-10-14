using System.Collections.Generic;
using System.Linq;
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
  private GameObject zoneInfoGuardMark;
  private GameObject zoneInfoClearedMark;
  private GameObject zoneInfoRecruitMark;
  private GameObject zoneInfoQuestMark;
  private GameObject zoneInfoCollectMark;
  private GameObject zoneInfoRespawnMark;

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
  private TextMeshProUGUI totalPeopleValue;
  private TextMeshProUGUI location;

  public string[] resTooltips = { "Wood", "Stone", "Metal", "Leather" };
  public Dictionary<MasteryLevel, Color> palette = new();

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
    zoneInfoGuardMark = Find(markers, "Guard");
    zoneInfoClearedMark = Find(markers, "Clear");
    zoneInfoRecruitMark = Find(markers, "Recruitment");
    zoneInfoQuestMark = Find(markers, "Quest");
    zoneInfoCollectMark = Find(markers, "Collecting");
    zoneInfoRespawnMark = Find(markers, "Respawning");

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
    totalPeopleValue = Get<TextMeshProUGUI>(resources, "TotalPeople/Value");
    location = Get<TextMeshProUGUI>(resources, "Location");

    if (!ComponentsInitialized()) {
      Debug.LogError("Map UI components initialization error");
      return;
    }

    palette = Utils.GetMasteryPalette();
    playerMenuButton.onClick.AddListener(SwitchPlayerMenu);
    EnableUI();
  }

  private bool ComponentsInitialized() {
    return new object[] {
      zoneInfoPanel, zoneInfoTitle, zoneInfoDescription,  zoneInfoQuestMark, playerMenuButton,
      goldValue,  woodValue, stoneValue, metalValue,  villagersValue,
      leatherValue, zoneInfoBattleMark,  zoneInfoClearedMark, zoneInfoRecruitMark, interactButton,
      location, interactButtonIcon, interactButtonText,  totalPeopleValue, zoneInfoCollectMark,
      zoneInfoRespawnMark, zoneInfoGuardMark
    }.All(x => x != null);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    playerMenuButton.onClick.RemoveListener(SwitchPlayerMenu);
  }

  public override void DisableUI() {
    base.DisableUI();
    playerMenuButton.interactable = false;
    interactButton.interactable = false;
  }

  public override void EnableUI() {
    base.EnableUI();
    playerMenuButton.interactable = true;
    interactButton.interactable = true;
  }

  protected override void OpenPauseMenu() {
    CloseOtherWindows();
    PlayerMenuUI.Close();
    AlmanacUI.Instance.Close();
    PauseMenu.Open();
  }

  private void SwitchPlayerMenu() {
    CloseOtherWindows();
    AlmanacUI.Instance.Close();
    PlayerMenuUI.Switch();
  }

  protected override void OpenAlmanac() {
    CloseOtherWindows();
    PlayerMenuUI.Close();
    AlmanacUI.Instance.Open();
  }

  private void CloseOtherWindows() {
    RecruitingUI.Close();
    HomeMenuUI.Close();
  }

  public void ShowZoneInfo(MapZone zone) {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(true);
    zoneInfoTitle.text = zone.zoneName;

    zoneInfoDescription.text = zone.events.Count == 0
      ? zone.descriptionCleared
      : zone.description;

    if (zone.events.Count == 0) {
      if (zone.descriptionCleared == "") {
        zoneInfoDescription.text = zone.description;
      } else {
        zoneInfoClearedMark.SetActive(true);
      }
    }
    else if (zone.events.Contains(MapZoneType.Battle)) {
      if (zone.TryGetComponent<MapZoneBattle>(out var battle)) {
        if (battle.instant) zoneInfoBattleMark.SetActive(true);
        else zoneInfoGuardMark.SetActive(true);
      }
    }
    else if (zone.events.Contains(MapZoneType.Recruitment)) zoneInfoRecruitMark.SetActive(true);
    else if (zone.events.Contains(MapZoneType.Quest)) zoneInfoQuestMark.SetActive(true);
    else if (zone.events.Contains(MapZoneType.Collecting)) {
      if (zone.TryGetComponent<MapZoneCollecting>(out var col)) {
        if (col.CollectedAt > 0 && col.CollectedAt + col.respawn > StateManager.globalTicks) {
          zoneInfoDescription.text = zone.descriptionCleared;
          zoneInfoRespawnMark.SetActive(true);
        } else {
          zoneInfoCollectMark.SetActive(true);
        }
      }
    }
  }

  public void HideZoneInfo() {
    if (!ComponentsInitialized()) return;
    zoneInfoPanel.SetActive(false);
    zoneInfoTitle.text = "";
    zoneInfoDescription.text = "";
    zoneInfoBattleMark.SetActive(false);
    zoneInfoGuardMark.SetActive(false);
    zoneInfoClearedMark.SetActive(false);
    zoneInfoRecruitMark.SetActive(false);
    zoneInfoQuestMark.SetActive(false);
    zoneInfoCollectMark.SetActive(false);
    zoneInfoRespawnMark.SetActive(false);
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
    villagersValue.text = player.Villagers.ToString();

    int totalPeople = player.GetTotalPeople().Sum();
    totalPeopleValue.text = string.Format(
      "{0} / {1}",
      totalPeople.ToString(),
      player.MaxVillagers.ToString()
    );
    if (totalPeople > player.MaxVillagers) {
      totalPeopleValue.text = "<color=#F61010>" + totalPeopleValue.text + "</color>";
    }
  }

  public void UpdateLocation(string value) {
    location.text = value;
  }
}
