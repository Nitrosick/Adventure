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
  public GameObject questSlotPrefab;
  public GameObject questEmptySlot;
  private static Transform menu;

  // Navigation
  private static Button navHero;
  private static Button navUnits;
  private static Button navInventory;
  private static Button navQuests;

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
  private static Transform abilities;

  // Quests
  private static Transform activeQuests;
  private static Transform completedQuests;
  private static GameObject activeQuestsEmpty;
  private static GameObject completedQuestsEmpty;
  private static Transform activeQuestsList;
  private static Transform completedQuestsList;

  private static readonly int slotColumns = 5;
  private static readonly float slotsGap = 4f;
  private static readonly float scrollWidth = 15f;
  private static readonly int defaultSlotsCount = 25;
  private static readonly int defaultQuestsInColumn = 7;
  private static float slotSize = 0;

  public static MenuSlot selectedSlot;
  public static Unit selectedUnit;
  public static SupportInstance selectedSupport;
  public static Item selectedItem;

  private void Awake() {
    Instance = this;
    menu = transform.Find("Menu/Player");

    Transform Find(string path) => menu.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    navHero = Get<Button>("Left/Navigation/Hero");
    navUnits = Get<Button>("Left/Navigation/Units");
    navInventory = Get<Button>("Left/Navigation/Inventory");
    navQuests = Get<Button>("Left/Navigation/Quests");

    leftSlots = Get<RectTransform>("Left/Blocks/Left/Slots/Viewport/Content");
    rightSlots = Get<RectTransform>("Left/Blocks/Right/Slots/Viewport/Content");
    leftSlotsTitle = Get<TextMeshProUGUI>("Left/Blocks/Left/Header/Title");
    rightSlotsTitle = Get<TextMeshProUGUI>("Left/Blocks/Right/Header/Title");
    playerProgress = Find("Left/Blocks/Left/PlayerProgress");

    Transform progressContent = Get<Transform>("Left/Blocks/Left/PlayerProgress/Viewport/Content");

    playerXpValue = Get<TextMeshProUGUI>("Left/Blocks/Left/PlayerProgress/Viewport/Content/Experience/Value");
    playerXpBar = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/ExperienceBar");
    playerXpBarFill = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/ExperienceBar/Fill");
    playerFameValue = Get<TextMeshProUGUI>("Left/Blocks/Left/PlayerProgress/Viewport/Content/Fame/Value");
    playerFameBar = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/FameBar");
    playerFameBarFill = Get<RectTransform>("Left/Blocks/Left/PlayerProgress/Viewport/Content/FameBar/Fill");

    abilities = Get<RectTransform>("Left/Blocks/Right/Abilities");
    activeQuests = Find("Left/Blocks/Left/ActiveQuests");
    completedQuests = Find("Left/Blocks/Right/CompletedQuests");
    activeQuestsEmpty = Find("Left/Blocks/Left/ActiveQuests/Viewport/Content/Empty").gameObject;
    completedQuestsEmpty = Find("Left/Blocks/Right/CompletedQuests/Viewport/Content/Empty").gameObject;
    activeQuestsList = Find("Left/Blocks/Left/ActiveQuests/Viewport/Content/List");
    completedQuestsList = Find("Left/Blocks/Right/CompletedQuests/Viewport/Content/List");

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI components initialization error");
      return;
    }

    navHero.onClick.AddListener(SelectHeroTab);
    navUnits.onClick.AddListener(SelectUnitsTab);
    navInventory.onClick.AddListener(SelectInventoryTab);
    navQuests.onClick.AddListener(SelectQuestsTab);
  }

  private async void Start() {
    await Task.Yield();
    await Task.Delay(10);
    UpdateSlotsSize(leftSlots);
    UpdateSlotsSize(rightSlots);
  }

  private static bool ComponentsInitialized() {
    return menu != null && leftSlots != null && rightSlots != null &&
      leftSlotsTitle != null && rightSlotsTitle != null && navHero != null &&
      navUnits != null && navInventory != null && playerProgress != null &&
      playerXpValue != null && playerXpBar != null && playerXpBarFill != null &&
      playerFameValue != null && playerFameBar != null && playerFameBarFill != null &&
      navQuests != null && activeQuests != null && completedQuests != null &&
      activeQuestsEmpty != null && completedQuestsEmpty != null && activeQuestsList != null &&
      completedQuestsList != null && abilities != null;
  }

  private void OnDestroy() {
    navHero.onClick.RemoveListener(SelectHeroTab);
    navUnits.onClick.RemoveListener(SelectUnitsTab);
    navInventory.onClick.RemoveListener(SelectInventoryTab);
    navQuests.onClick.RemoveListener(SelectQuestsTab);
  }

  public static void Open() {
    menu.gameObject.SetActive(true);
    SelectHeroTab();
    SceneController.OpenWindow("player");
  }

  public static void Close() {
    menu.gameObject.SetActive(false);
    Clear();
    PlayerMenuUIFilters.value = MenuFilter.All;
    SceneController.CloseWindow("player");
  }

  public static void Switch() {
    if (menu.gameObject.activeSelf) Close();
    else Open();
  }

  private static void Clear() {
    navHero.interactable = true;
    navUnits.interactable = true;
    navInventory.interactable = true;
    navQuests.interactable = true;

    foreach (Transform child in leftSlots) Destroy(child.gameObject);
    foreach (Transform child in rightSlots) Destroy(child.gameObject);
    foreach (Transform child in activeQuestsList) Destroy(child.gameObject);
    foreach (Transform child in completedQuestsList) Destroy(child.gameObject);

    ShowSlots(false);
    playerProgress.gameObject.SetActive(false);
    abilities.gameObject.SetActive(false);
    activeQuests.gameObject.SetActive(false);
    completedQuests.gameObject.SetActive(false);

    leftSlotsTitle.text = "";
    rightSlotsTitle.text = "";
    PlayerMenuUIInfo.Clear();
    PlayerMenuUIFilters.Reset();

    selectedSlot = null;
    selectedUnit = null;
    selectedSupport = null;
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
    rightSlotsTitle.text = "Abilities";
    Player player = Player.Instance;

    playerProgress.gameObject.SetActive(true);
    abilities.gameObject.SetActive(true);
    PlayerMenuUIAbilities.Init();
    PlayerMenuUIAchievements.Init();

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
    ShowDefaultInfo();
  }

  public async static void SelectUnitsTab() {
    Clear();
    navUnits.interactable = false;
    ShowSlots(true);

    PlayerMenuUIFilters.InitUnitFilters();
    MenuFilter filter = PlayerMenuUIFilters.value;
    Player player = Player.Instance;

    Unit[] units = player.Army.Units
      .Where(u => {
        if (
          u.IsHero ||
          (filter == MenuFilter.FreeUnits && u.InSquad) ||
          (filter == MenuFilter.UnitsInSquad && !u.InSquad)
        ) return false;
        return true;
      })
      .ToArray();

    SupportInstance[] supports = player.Army.Supports
      .Where(s => {
        if (
          (filter == MenuFilter.FreeSupports && s.inSquad) ||
          (filter == MenuFilter.SupportsInSquad && !s.inSquad)
        ) return false;
        return true;
      })
      .ToArray();

    int unitsInSquad = player.Army.Units.Where(u => u.InSquad).ToArray().Length;
    int supportsInSquad = player.Army.Supports.Where(u => u.inSquad).ToArray().Length;

    leftSlotsTitle.text = $"Army ({unitsInSquad} in squad)";
    rightSlotsTitle.text = $"Supports ({supportsInSquad} / {player.Army.SupportSlots} in squad)";

    PlayerMenuUIInfo.UnitDismiss.interactable = units.Length > 1;

    foreach (Unit unit in units) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, leftSlots);
      slot.GetComponent<MenuSlot>().Init(unit);
    }

    foreach (SupportInstance support in supports) {
      GameObject slot = Instantiate(Instance.menuSlotPrefab, rightSlots);
      slot.GetComponent<MenuSlot>().Init(support);
    }

    RenderEmptySlots(leftSlots, units.Length);
    RenderEmptySlots(rightSlots, supports.Length);

    await Task.Yield();
    selectedSlot = leftSlots.GetChild(0).GetComponent<MenuSlot>();
    if (selectedSlot != null) PlayerMenuUIInfo.ShowInfo(selectedSlot.UnitItem);
    else ShowDefaultInfo();
  }

  public async static void SelectInventoryTab() {
    Clear();
    navInventory.interactable = false;
    ShowSlots(true);
    leftSlotsTitle.text = "Equipment";
    rightSlotsTitle.text = "Miscellaneous items";

    PlayerMenuUIFilters.InitInventoryFilters();
    MenuFilter filter = PlayerMenuUIFilters.value;
    Player player = Player.Instance;

    Equipment[] allEquip = player.Inventory.Equip
      .Where(e => {
        if (
          (filter == MenuFilter.Weapon && e is not Weapon) ||
          (filter == MenuFilter.Armor && e is not Armor) ||
          (filter == MenuFilter.Additional && e is not AdditionalItem)
        ) return false;
        return true;
      })
      .ToArray();

    List<Equipment> unequipped = allEquip.ToList();
    List<Equipment> equipped = new();

    foreach (Unit unit in player.Army.Units)
      equipped.AddRange(unit.Equip.GetEquipmentList(filter));

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
      .Where(i => {
        if (filter == MenuFilter.Medicine && i is not MedicineItem) return false;
        return true;
      })
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
    else ShowDefaultInfo();
  }

  private async static void SelectQuestsTab() {
    Clear();
    navQuests.interactable = false;
    activeQuests.gameObject.SetActive(true);
    completedQuests.gameObject.SetActive(true);
    leftSlotsTitle.text = "Active";
    rightSlotsTitle.text = "Completed";

    List<QuestInstance> quests = QuestManager.questsList;
    QuestInstance[] active = quests.Where(q => q.state == QuestState.Accepted).ToArray();
    QuestInstance[] completed = quests.Where(q => q.state == QuestState.Completed).ToArray();

    activeQuestsList.gameObject.SetActive(active.Length > 0);
    completedQuestsList.gameObject.SetActive(completed.Length > 0);
    activeQuestsEmpty.SetActive(active.Length == 0);
    completedQuestsEmpty.SetActive(completed.Length == 0);

    foreach (QuestInstance q in active) {
      GameObject slot = Instantiate(Instance.questSlotPrefab, activeQuestsList);
      slot.GetComponent<QuestSlot>().Init(q);
    }

    foreach (QuestInstance q in completed) {
      GameObject slot = Instantiate(Instance.questSlotPrefab, completedQuestsList);
      slot.GetComponent<QuestSlot>().Init(q);
    }

    RenderQuestEmptySlots(activeQuestsList, active.Length);
    RenderQuestEmptySlots(completedQuestsList, completed.Length);

    await Task.Yield();
    ShowDefaultInfo();
  }

  private static void RenderEmptySlots(RectTransform panel, int filled) {
    if (filled == defaultSlotsCount) {
      return;
    } else if (filled < defaultSlotsCount) {
      for (int i = filled; i < defaultSlotsCount; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, panel);
      }
    } else {
      int remainder = filled % slotColumns;
      int placeholders = remainder == 0 ? 0 : slotColumns - remainder;

      for (int i = 0; i < placeholders; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, panel);
      }
    }
  }

  private static void RenderQuestEmptySlots(Transform panel, int filled) {
    if (filled >= defaultQuestsInColumn) return;

    for (int i = 0; i < defaultQuestsInColumn - filled; i++) {
      Instantiate(Instance.questEmptySlot, panel);
    }
  }

  public static void FrameSlot() {
    MenuSlot[] allSlots = FindObjectsOfType<MenuSlot>();
    if (allSlots.Length > 0) {
      foreach (MenuSlot slot in allSlots) slot.SwitchActiveFrame(false);
      if (selectedSlot != null) selectedSlot.SwitchActiveFrame(true);
    }
  }

  private static void ShowDefaultInfo() {
    Unit hero = Player.Instance.Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    PlayerMenuUIInfo.ShowInfo(hero);
  }
}
