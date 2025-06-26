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
  private static Transform info;

  // Navigation
  private static Button navHero;
  private static Button navUnits;
  private static Button navInventory;

  // Slots
  private static RectTransform leftSlots;
  private static RectTransform rightSlots;
  private static TextMeshProUGUI leftSlotsTitle;
  private static TextMeshProUGUI rightSlotsTitle;

  // Info
  private static TextMeshProUGUI infoName;
  private static TextMeshProUGUI infoLevel;
  private static TextMeshProUGUI infoType;
  private static Transform infoAvatar;
  private static GameObject infoActions;
  private static Button unitInSquad;
  private static Button unitDismiss;

  private static readonly int slotColumns = 5;
  private static readonly float slotsGap = 4f;
  private static readonly float scrollWidth = 15f;
  private static readonly int defaultSlotsCount = 25;

  private static Unit selectedUnit;

  private void Awake() {
    Instance = this;
    menu = transform.Find("PlayerMenu/Panel");
    info = transform.Find("PlayerMenu/Panel/Right/Viewport/Content");

    navHero = menu.Find("Left/Navigation/Hero").GetComponent<Button>();
    navUnits = menu.Find("Left/Navigation/Units").GetComponent<Button>();
    navInventory = menu.Find("Left/Navigation/Inventory").GetComponent<Button>();

    leftSlots = menu.Find("Left/Blocks/Left/Slots/Viewport/Content").GetComponent<RectTransform>();
    rightSlots = menu.Find("Left/Blocks/Right/Slots/Viewport/Content").GetComponent<RectTransform>();
    leftSlotsTitle = menu.Find("Left/Blocks/Left/Title").GetComponent<TextMeshProUGUI>();
    rightSlotsTitle = menu.Find("Left/Blocks/Right/Title").GetComponent<TextMeshProUGUI>();

    infoName = info.Find("Head/Data/Name").GetComponent<TextMeshProUGUI>();
    infoLevel = info.Find("Head/Data/Level").GetComponent<TextMeshProUGUI>();
    infoType = info.Find("Head/Data/Type").GetComponent<TextMeshProUGUI>();
    infoAvatar = info.Find("Head/Image").GetComponent<Transform>();
    infoActions = info.Find("Actions").gameObject;
    unitInSquad = info.Find("Actions/Activity").GetComponent<Button>();
    unitDismiss = info.Find("Actions/Dismiss").GetComponent<Button>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI components initialization error");
    }

    navHero.onClick.AddListener(SelectHeroTab);
    navUnits.onClick.AddListener(SelectUnitsTab);
    navInventory.onClick.AddListener(SelectInventoryTab);
    unitInSquad.onClick.AddListener(SwitchUnitInSquad);
  }

  private async void Start() {
    await Task.Yield();
    await Task.Delay(10);
    UpdateSlotsSize(leftSlots);
    UpdateSlotsSize(rightSlots);
  }

  private static bool ComponentsInitialized() {
    return menu != null && info != null && leftSlots != null &&
    rightSlots != null && leftSlotsTitle != null && rightSlotsTitle != null &&
    navHero != null && navUnits != null && navInventory != null &&
    infoName != null && infoLevel != null && infoType != null &&
    infoAvatar != null && infoActions != null && unitInSquad != null &&
    unitDismiss != null;
  }

  private void OnDestroy() {
    navHero.onClick.RemoveListener(SelectHeroTab);
    navUnits.onClick.RemoveListener(SelectUnitsTab);
    navInventory.onClick.RemoveListener(SelectInventoryTab);
    unitInSquad.onClick.RemoveListener(SwitchUnitInSquad);
  }

  public static void Switch() {
    GameObject menuObj = menu.gameObject;
    menuObj.SetActive(!menuObj.activeSelf);
    if (menuObj.activeSelf) SelectHeroTab();
    else Clear();
  }

  private static void Clear() {
    navHero.interactable = true;
    navUnits.interactable = true;
    navInventory.interactable = true;
    foreach (Transform child in leftSlots) Destroy(child.gameObject);
    foreach (Transform child in rightSlots) Destroy(child.gameObject);
    leftSlotsTitle.text = "";
    rightSlotsTitle.text = "";

    infoName.text = "-";
    infoLevel.text = "Level: -";
    infoType.text = "Type: -";
    infoActions.SetActive(false);

    foreach (Transform child in infoAvatar) {
      Destroy(child.gameObject);
    }

    selectedUnit = null;
  }

  private void UpdateSlotsSize(RectTransform slots) {
    GridLayoutGroup gridGroup = slots.GetComponent<GridLayoutGroup>();

    float totalWidth = slots.rect.width - scrollWidth * 2;
    float totalSpacing = slotsGap * (slotColumns - 1) + slotsGap * 2;
    float cellSize = (totalWidth - totalSpacing) / slotColumns;

    gridGroup.cellSize = new Vector2(cellSize, cellSize);
  }

  private static void SelectHeroTab() {
    Clear();
    navHero.interactable = false;
    leftSlotsTitle.text = "Progress";
    rightSlotsTitle.text = "Skills";

    Player player = Player.Instance;
    if (player == null) return;
    infoLevel.text = "Level: " + player.Level.ToString();
    Unit hero = player.Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    ShowInfo(hero);
  }

  public static void SelectUnitsTab() {
    Clear();
    navUnits.interactable = false;
    leftSlotsTitle.text = "Army";
    rightSlotsTitle.text = "Workers";

    Unit[] units = Player.Instance.Army.Units
      .Where(u => !u.IsHero).ToArray();

    foreach (Unit unit in units) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, leftSlots);
      slot.GetComponent<MenuSlot>().Init(unit);
    }

    RenderEmptySlots(leftSlots, units.Length);
    RenderEmptySlots(rightSlots, 0);
    infoActions.SetActive(true);
    if (units.Length > 0) ShowInfo(units[0]);
  }

  private static void SelectInventoryTab() {
    Clear();
    navInventory.interactable = false;
    leftSlotsTitle.text = "Equipment";
    rightSlotsTitle.text = "Key items";

    RenderEmptySlots(leftSlots, 0);
    RenderEmptySlots(rightSlots, 0);
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

  public static void ShowInfo(Unit unit) {
    foreach (Transform child in infoAvatar) {
      Destroy(child.gameObject);
    }

    selectedUnit = unit;
    // FIXME: Добавить все поля юнита
    infoName.text = unit.Name;
    if (!unit.IsHero) infoLevel.text = "Level: " + unit.Level.ToString();
    infoType.text = "Type: " + unit.Type.ToString();

    GameObject avatar = Instantiate(Instance.menuSlotPrefab, infoAvatar);
    avatar.GetComponent<MenuSlot>().Init(unit, true);

    unitInSquad.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = unit.InSquad
      ? "Remove from squad"
      : "Add to squad";
  }

  public static void ShowInfo(Equipment equip) {

  }

  private static void SwitchUnitInSquad() => Player.Instance.Army.SwitchUnitInSquad(selectedUnit);

  // public static void DisableUI() {
  //   button.interactable = false;
  // }

  // public static void EnableUI() {
  //   button.interactable = true;
  // }
}
