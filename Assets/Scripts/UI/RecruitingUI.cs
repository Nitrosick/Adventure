using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitingUI : MonoBehaviour {
  public static RecruitingUI Instance;
  public GameObject slotPrefab;

  private static Transform window;
  private static Button submit;
  private static Button cancel;
  private static GameObject notEnoughRes;
  private static GameObject notEnoughSlots;
  private static MapZoneRecruitment mapZone;

  // Reward
  private static TextMeshProUGUI rewardTitle;
  private static Transform rewardSlots;

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

    window = transform.Find("Recruitment/Panel");
    submit = Get<Button>("Control/Recruit");
    cancel = Get<Button>("Control/Cancel");
    notEnoughRes = Find("NotEnoughRes").gameObject;
    notEnoughSlots = Find("NotEnoughSlots").gameObject;

    rewardTitle = Get<TextMeshProUGUI>("Reward/Title");
    rewardSlots = Find("Reward/Slots");

    reqPlayerLevel = Get<TextMeshProUGUI>("Requirements/PlayerLevel/Value");
    reqPlayerFame = Get<TextMeshProUGUI>("Requirements/PlayerFame/Value");
    reqGold = Get<TextMeshProUGUI>("Requirements/Gold/Value");
    requirementSlots = Find("Requirements/Slots");

    if (
      window == null || submit == null || cancel == null ||
      notEnoughRes == null || notEnoughSlots == null || rewardTitle == null ||
      rewardSlots == null || reqPlayerLevel == null || reqPlayerFame == null ||
      reqGold == null || requirementSlots == null
    ) {
      Debug.LogError("Recruiting UI components initialization error");
    }

    submit.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    submit.onClick.RemoveListener(OnSubmit);
    cancel.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneRecruitment zone) {
    if (zone == null || zone.requirements == null) return;
    mapZone = zone;
    ShowRequirements();
    RenderSlots();

    if (!EnoughSlots(zone.recruitVillagers)) {
      notEnoughSlots.SetActive(true);
      submit.interactable = false;
    } else if (!MeetsRequirements(zone.requirements)) {
      notEnoughRes.SetActive(true);
      submit.interactable = false;
    }

    window.gameObject.SetActive(true);
    SceneController.OpenWindow("recruiting");
  }

  public static void Close() {
    window.gameObject.SetActive(false);

    mapZone = null;
    notEnoughRes.SetActive(false);
    notEnoughSlots.SetActive(false);
    submit.interactable = true;

    rewardTitle.text = "";
    reqPlayerLevel.text = "-";
    reqPlayerFame.text = "-";
    reqGold.text = "-";

    ClearSlots();
    SceneController.CloseWindow("recruiting");
  }

  private static void ClearSlots() {
    foreach (Transform child in rewardSlots) Destroy(child.gameObject);
    foreach (Transform child in requirementSlots) Destroy(child.gameObject);
  }

  private static void ShowRequirements() {
    rewardTitle.text = mapZone.recruitVillagers > 0 ? "Villagers" : "Units";
    Requirements req = mapZone.requirements;
    if (req.playerLevel > 0) reqPlayerLevel.text = req.playerLevel.ToString();
    if (req.playerFame > 0) reqPlayerFame.text = req.playerFame.ToString();
    if (req.gold > 0) reqGold.text = req.gold.ToString();
  }

  private static void RenderSlots() {
    if (mapZone.recruitVillagers > 0) {
      GameObject slot = Instantiate(Instance.slotPrefab, rewardSlots);
      slot.GetComponent<SlotWithCount>().Init(
        MapUI.Instance.villagersSprite,
        mapZone.recruitVillagers,
        "Villagers"
      );
    }

    if (mapZone.recruits.Length > 0) {
      foreach (Unit unit in mapZone.recruits) {
        GameObject slot = Instantiate(Instance.slotPrefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(unit);
      }
    }

    Requirements req = mapZone.requirements;
    int slotsCount = 0;

    requirementSlots.gameObject.SetActive(
      req.resources.Any(x => x > 0) ||
      req.equipment.Length > 0 ||
      req.items.Length > 0
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
      req.gold > player.Gold
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

  private static bool EnoughSlots(int count) {
    Player player = Player.Instance;
    return player.GetTotalPeople().Sum() + count <= player.MaxVillagers;
  }

  private static void OnSubmit() {
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
    foreach (Equipment item in req.equipment) player.Inventory.RemoveItem(item);
    foreach (Item item in req.items) player.Inventory.RemoveItem(item);
    player.SetVillagers(mapZone.recruitVillagers);
    foreach (Unit unit in mapZone.recruits) player.Army.AddUnit(unit);

    mapZone.GetComponent<MapZone>().RemoveEvent(MapZoneType.Recruitment);
    _ = Toast.Show("success", "People have joined you");
    Close();
  }
}
