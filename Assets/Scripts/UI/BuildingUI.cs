using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour {
  public static BuildingUI Instance;
  public GameObject slotPrefab;

  private static Transform window;
  private static Button submit;
  private static Button cancel;
  private static GameObject notEnoughRes;
  private static MapZoneBuilding mapZone;

  // Reward
  private static TextMeshProUGUI rewardTitle;
  private static Image rewardImage;

  // Requirements
  private static TextMeshProUGUI reqPlayerLevel;
  private static TextMeshProUGUI reqPlayerFame;
  private static TextMeshProUGUI reqGold;
  private static Transform requirementSlots;

  private static readonly int slotsInRow = 5;

  private void Awake() {
    Instance = this;

    Transform Find(string path) => window.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    window = transform.Find("Building/Panel");
    submit = Get<Button>("Control/Build");
    cancel = Get<Button>("Control/Cancel");
    notEnoughRes = Find("NotEnoughRes").gameObject;

    rewardTitle = Get<TextMeshProUGUI>("Reward/Title");
    rewardImage = Get<Image>("Reward/Image/Image");

    reqPlayerLevel = Get<TextMeshProUGUI>("Requirements/PlayerLevel/Value");
    reqPlayerFame = Get<TextMeshProUGUI>("Requirements/PlayerFame/Value");
    reqGold = Get<TextMeshProUGUI>("Requirements/Gold/Value");
    requirementSlots = Find("Requirements/Slots");

    if (
      window == null || submit == null || cancel == null ||
      notEnoughRes == null || rewardTitle == null || rewardImage == null ||
      reqPlayerLevel == null || reqPlayerFame == null || reqGold == null ||
      requirementSlots == null
    ) {
      Debug.LogError("Building UI components initialization error");
    }

    submit.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    submit.onClick.RemoveListener(OnSubmit);
    cancel.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneBuilding zone) {
    ClearSlots();
    if (zone == null || zone.requirements == null || zone.sprite == null) return;
    mapZone = zone;
    rewardImage.sprite = zone.sprite;
    ShowRequirements();
    RenderSlots();

    if (!MeetsRequirements(zone.requirements)) {
      notEnoughRes.SetActive(true);
      submit.interactable = false;
    }

    window.gameObject.SetActive(true);
    SceneController.OpenWindow("building");
  }

  public static void Close() {
    window.gameObject.SetActive(false);

    mapZone = null;
    notEnoughRes.SetActive(false);
    submit.interactable = true;

    rewardTitle.text = "";
    reqPlayerLevel.text = "-";
    reqPlayerFame.text = "-";
    reqGold.text = "-";

    ClearSlots();
    SceneController.CloseWindow("building");
  }

  private static void ClearSlots() {
    foreach (Transform child in requirementSlots) Destroy(child.gameObject);
  }

  private static void ShowRequirements() {
    rewardTitle.text = mapZone.building.ToString();
    Requirements req = mapZone.requirements;
    if (req.playerLevel > 0) reqPlayerLevel.text = req.playerLevel.ToString();
    if (req.playerFame > 0) reqPlayerFame.text = req.playerFame.ToString();
    if (req.gold > 0) reqGold.text = req.gold.ToString();
  }

  private static void RenderSlots() {
    Requirements req = mapZone.requirements;
    int slotsCount = 0;

    requirementSlots.gameObject.SetActive(
      req.resources.Any(x => x > 0) ||
      req.equipment.Length > 0 ||
      req.items.Length > 0 ||
      req.villagers > 0
    );

    for (int i = 0; i < req.resources.Length; i++) {
      if (req.resources[i] > 0) {
        GameObject slot = Instantiate(Instance.slotPrefab, requirementSlots);
        slot.GetComponent<SlotWithCount>().Init(
          MapUI.Instance.resourceSprites[i],
          req.resources[i],
          MapUI.Instance.resTooltips[i]
        );
        slotsCount++;
      }
    }

    if (req.villagers > 0) {
      GameObject slot = Instantiate(Instance.slotPrefab, requirementSlots);
      slot.GetComponent<SlotWithCount>().Init(
        MapUI.Instance.villagersSprite,
        req.villagers,
        "Villagers"
      );
      slotsCount++;
    }

    if (req.equipment.Length > 0) {
      foreach (Equipment item in req.equipment) {
        GameObject slot = Instantiate(Instance.slotPrefab, requirementSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    if (req.items.Length > 0) {
      foreach (Item item in req.items) {
        GameObject slot = Instantiate(Instance.slotPrefab, requirementSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    RenderEmptySlots(slotsCount);
  }

  private static void RenderEmptySlots(int filled) {
    if (filled == slotsInRow) {
      return;
    } else if (filled < slotsInRow) {
      for (int i = filled; i < slotsInRow; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, requirementSlots);
      }
    } else {
      int remainder = filled % slotsInRow;
      int placeholders = remainder == 0 ? 0 : slotsInRow - remainder;

      for (int i = 0; i < placeholders; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, requirementSlots);
      }
    }
  }

  private static bool MeetsRequirements(Requirements req) {
    Player player = Player.Instance;
    bool check = true;

    if (
      req.playerLevel > player.Level ||
      req.playerFame > player.Fame ||
      req.gold > player.Gold ||
      req.villagers > player.Villagers
    ) check = false;

    for (int i = 0; i < req.resources.Length; i++) {
      if (req.resources[i] > player.Resources[i]) check = false;
    }

    if (req.equipment.Length > 0) {
      foreach (Equipment item in req.equipment) {
        if (!player.Inventory.HasItem(item)) check = false;
      }
    }

    if (req.items.Length > 0) {
      foreach (Item item in req.items) {
        if (!player.Inventory.HasItem(item)) check = false;
      }
    }

    return check;
  }

  private async static void OnSubmit() {
    Player player = Player.Instance;
    Requirements req = mapZone.requirements;

    if (req.equipment.Length > 0) {
      foreach (Equipment item in req.equipment) {
        if (!player.Inventory.HasItem(item, true)) {
          _ = Toast.Show("warning", "The required item is equipped on the unit");
          return;
        }
      }
    }

    player.SetGold(req.gold * -1);
    player.SetResources(req.resources.Select(n => -n).ToArray());
    player.SetVillagers(req.villagers * -1);
    foreach (Equipment item in req.equipment) player.Inventory.RemoveItem(item);
    foreach (Item item in req.items) player.Inventory.RemoveItem(item);

    Close();
    SceneController.ShowEventInfo("build", "Building");
    await SceneController.Fade(0f, 1f, true);

    MapZone parentZone = mapZone.GetComponent<MapZone>();
    if (mapZone.building == Building.Watchtower) parentZone.RemoveEvent(MapZoneType.Ambush);
    parentZone.RemoveEvent(MapZoneType.Constructing);

    await SceneController.Fade(1f, 0f, false);
    SceneController.HideEventInfo();
    StateManager.SaveGame();
  }
}
