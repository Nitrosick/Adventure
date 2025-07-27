using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuUI : MonoBehaviour {
  // Components
  public static PlayerMenuUI Instance;

  public GameObject menuSlotPrefab;
  public GameObject menuEmptySlotPrefab;
  private static Transform menu;

  // Navigation
  private static Button navHero;
  private static Button navUnits;
  private static Button navInventory;

  // Slots
  private static RectTransform leftSlots;
  private static RectTransform rightSlots;
  private static TextMeshProUGUI leftSlotsTitle;
  private static TextMeshProUGUI rightSlotsTitle;

  // Progress
  private static Transform playerProgress;
  private static TextMeshProUGUI playerXpValue;
  private static RectTransform playerXpBar;
  private static RectTransform playerXpBarFill;
  private static TextMeshProUGUI playerFameValue;
  private static RectTransform playerFameBar;
  private static RectTransform playerFameBarFill;

  private static readonly int slotColumns = 5;
  private static readonly float slotsGap = 4f;
  private static readonly float scrollWidth = 15f;
  private static readonly int defaultSlotsCount = 25;
  private static float slotSize = 0;

  public static MenuSlot selectedSlot;
  public static Unit selectedUnit;
  public static Item selectedItem;

  private void Awake() {
    Instance = this;
    menu = transform.Find("PlayerMenu/Panel");

    Transform Find(string path) => menu.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    navHero = Get<Button>("Left/Navigation/Hero");
    navUnits = Get<Button>("Left/Navigation/Units");
    navInventory = Get<Button>("Left/Navigation/Inventory");

    leftSlots = Get<RectTransform>("Left/Blocks/Left/Slots/Viewport/Content");
    rightSlots = Get<RectTransform>("Left/Blocks/Right/Slots/Viewport/Content");
    leftSlotsTitle = Get<TextMeshProUGUI>("Left/Blocks/Left/Title");
    rightSlotsTitle = Get<TextMeshProUGUI>("Left/Blocks/Right/Title");
    playerProgress = Find("Left/Blocks/Left/PlayerProgress");

    Transform progressContent = Get<Transform>("Left/Blocks/Left/PlayerProgress/Viewport/Content");

    playerXpValue = Get<TextMeshProUGUI>("Left/Blocks/Left/PlayerProgress/Viewport/Content/Experience/Value");
    playerXpBar = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/ExperienceBar");
    playerXpBarFill = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/ExperienceBar/Fill");
    playerFameValue = Get<TextMeshProUGUI>("Left/Blocks/Left/PlayerProgress/Viewport/Content/Fame/Value");
    playerFameBar = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/FameBar");
    playerFameBarFill = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/FameBar/Fill");

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI components initialization error");
      return;
    }

    navHero.onClick.AddListener(SelectHeroTab);
    navUnits.onClick.AddListener(SelectUnitsTab);
    navInventory.onClick.AddListener(SelectInventoryTab);
  }

  private async void Start() {
    await Task.Yield();
    await Task.Delay(10);
    UpdateSlotsSize(leftSlots);
    UpdateSlotsSize(rightSlots);
    PlayerMenuUIInfo.UpdateSlotsSize(slotSize);
  }

  private static bool ComponentsInitialized() {
    return menu != null && leftSlots != null && rightSlots != null &&
    leftSlotsTitle != null && rightSlotsTitle != null && navHero != null &&
    navUnits != null && navInventory != null && playerProgress != null &&
    playerXpValue != null && playerXpBar != null && playerXpBarFill != null &&
    playerFameValue != null && playerFameBar != null && playerFameBarFill != null;
  }

  private void OnDestroy() {
    navHero.onClick.RemoveListener(SelectHeroTab);
    navUnits.onClick.RemoveListener(SelectUnitsTab);
    navInventory.onClick.RemoveListener(SelectInventoryTab);
  }

  public static void Open() {
    menu.gameObject.SetActive(true);
    SceneController.ShowBackground();
    SelectHeroTab();
  }

  public static void Close() {
    menu.gameObject.SetActive(false);
    Clear();
    SceneController.HideBackground();
  }

  public static void Switch() {
    if (menu.gameObject.activeSelf) Close();
    else Open();
  }

  private static void Clear() {
    navHero.interactable = true;
    navUnits.interactable = true;
    navInventory.interactable = true;
    foreach (Transform child in leftSlots) Destroy(child.gameObject);
    foreach (Transform child in rightSlots) Destroy(child.gameObject);
    ShowSlots(false);
    playerProgress.gameObject.SetActive(false);

    leftSlotsTitle.text = "";
    rightSlotsTitle.text = "";
    PlayerMenuUIInfo.Clear();

    selectedSlot = null;
    selectedUnit = null;
    selectedItem = null;
  }

  private static void UpdateSlotsSize(RectTransform slots) {
    GridLayoutGroup gridGroup = slots.GetComponent<GridLayoutGroup>();
    float totalWidth = slots.rect.width - scrollWidth * 2;
    float totalSpacing = slotsGap * (slotColumns - 1) + slotsGap * 2;
    slotSize = (totalWidth - totalSpacing) / slotColumns;
    gridGroup.cellSize = new Vector2(slotSize, slotSize);
  }

  private static void ShowSlots(bool on) {
    GameObject l = menu.Find("Left/Blocks/Left/Slots").gameObject;
    GameObject r = menu.Find("Left/Blocks/Right/Slots").gameObject;
    l.SetActive(on);
    r.SetActive(on);
  }

  public async static void SelectHeroTab() {
    Clear();
    navHero.interactable = false;
    leftSlotsTitle.text = "Progress";
    rightSlotsTitle.text = "Skills";

    Player player = Player.Instance;
    Unit hero = player.Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;

    PlayerMenuUIInfo.Level.text = "Level: " + player.Level.ToString();
    PlayerMenuUIInfo.SelectHeroTab();
    playerProgress.gameObject.SetActive(true);

    playerXpValue.text = string.Format(
      "{0} / {1} (Level {2})",
      player.Experience,
      player.XPForNextLevel,
      player.Level
    );
    playerFameValue.text = player.Fame.ToString();

    float barsWidth = Mathf.Abs(playerXpBar.rect.width) - 8f;
    float xpPercent = Mathf.Clamp01((float)player.Experience / player.XPForNextLevel);
    playerXpBarFill.sizeDelta = new Vector2(barsWidth * xpPercent, playerXpBarFill.sizeDelta.y);
    float famePercent = Mathf.Clamp01((float)player.Fame / player.MaxFame);
    playerFameBarFill.sizeDelta = new Vector2(barsWidth * famePercent, playerFameBarFill.sizeDelta.y);

    await Task.Yield();
    PlayerMenuUIInfo.ShowInfo(hero);
  }

  public async static void SelectUnitsTab() {
    Clear();
    navUnits.interactable = false;
    ShowSlots(true);
    leftSlotsTitle.text = "Army";
    rightSlotsTitle.text = "Workers";

    Player player = Player.Instance;
    Unit[] units = player.Army.Units
      .Where(u => !u.IsHero).ToArray();

    PlayerMenuUIInfo.UnitDismiss.interactable = units.Length > 1;

    foreach (Unit unit in units) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, leftSlots);
      slot.GetComponent<MenuSlot>().Init(unit);
    }

    RenderEmptySlots(leftSlots, units.Length);
    RenderEmptySlots(rightSlots, 0);

    await Task.Yield();
    selectedSlot = leftSlots.GetChild(0).GetComponent<MenuSlot>();
    if (selectedSlot != null) PlayerMenuUIInfo.ShowInfo(selectedSlot.UnitItem);
  }

  public async static void SelectInventoryTab() {
    Clear();
    navInventory.interactable = false;
    ShowSlots(true);
    leftSlotsTitle.text = "Equipment";
    rightSlotsTitle.text = "Miscellaneous items";

    Player player = Player.Instance;
    List<Equipment> unequipped = player.Inventory.Equip;
    List<Equipment> equipped = new();

    foreach (Unit unit in player.Army.Units)
      equipped.AddRange(unit.Equip.GetEquipmentList());

    var groupedUnequipped = unequipped
      .GroupBy(e => e.id)
      .Select(g => new {
        equip = g.First(),
        count = g.Count()
      });

    foreach (var g in groupedUnequipped) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, leftSlots);
      slot.GetComponent<MenuSlot>().Init(g.equip, false, g.count);
    }

    var groupedEquipped = equipped
      .GroupBy(e => e.id)
      .Select(g => new {
        equip = g.First(),
        count = g.Count()
      });

    foreach (var g in groupedEquipped) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, leftSlots);
      MenuSlot slotScript = slot.GetComponent<MenuSlot>();
      slotScript.Init(g.equip, false, g.count);
      slotScript.SwitchActiveMark();
    }

    var groupedItems = player.Inventory.Items
      .GroupBy(item => item.id)
      .Select(group => new {
        item = group.First(),
        count = group.Count()
      });

    foreach (var g in groupedItems) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, rightSlots);
      var slotScript = slot.GetComponent<MenuSlot>();
      slot.GetComponent<MenuSlot>().Init(g.item, false, g.count);
    }

    RenderEmptySlots(leftSlots, groupedEquipped.ToArray().Length + groupedUnequipped.ToArray().Length);
    RenderEmptySlots(rightSlots, groupedItems.ToArray().Length);

    await Task.Yield();
    selectedSlot = leftSlots.GetChild(0).GetComponent<MenuSlot>();
    if (selectedSlot != null) PlayerMenuUIInfo.ShowInfo(selectedSlot.EquipmentItem);
  }

  private static void RenderEmptySlots(RectTransform panel, int filled) {
    if (filled == defaultSlotsCount) {
      return;
    }
    else if (filled < defaultSlotsCount) {
      for (int i = filled; i < defaultSlotsCount; i++) {
        Instantiate(Instance.menuEmptySlotPrefab, panel);
      }
    }
    else {
      int remainder = filled % slotColumns;
      int placeholders = remainder == 0 ? 0 : slotColumns - remainder;

      for (int i = 0; i < placeholders; i++) {
        Instantiate(Instance.menuEmptySlotPrefab, panel);
      }
    }
  }

  public static void FrameSlot() {
    MenuSlot[] allSlots = FindObjectsOfType<MenuSlot>();
    if (allSlots.Length > 0) {
      foreach (MenuSlot slot in allSlots) slot.SwitchActiveFrame(false);
      if (selectedSlot != null) selectedSlot.SwitchActiveFrame(true);
    }
  }
}
