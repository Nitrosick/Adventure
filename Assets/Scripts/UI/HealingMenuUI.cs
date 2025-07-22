using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealingMenuUI : MonoBehaviour {
  public GameObject slotPrefab;
  public GameObject slotEmptyPrefab;

  private TextMeshProUGUI title;
  private TextMeshProUGUI level;
  private TextMeshProUGUI intensity;
  private TextMeshProUGUI reanimationChance;
  private Image avatarBackground;
  private TextMeshProUGUI healCost;
  private RectTransform woundedSlots;
  private TextMeshProUGUI reanimationCost;
  private RectTransform deadSlots;

  private readonly Dictionary<MasteryLevel, Color> palette = new();
  private readonly Dictionary<MasteryLevel, int[]> healValues = new();
  private readonly Dictionary<MasteryLevel, int> reanimationChances = new();

  private static readonly float baseHealCost = 3f;
  private static readonly float healCostModifier = 1.4f;
  private static readonly float baseReanimationCost = 6f;
  private static readonly float reanimationCostModifier = 1.3f;
  private static readonly int slotColumns = 5;
  private static readonly float slotsGap = 4f;

  private void Awake() {
    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    title = Get<TextMeshProUGUI>("Head/Data/Name");
    level = Get<TextMeshProUGUI>("Head/Data/Level");
    intensity = Get<TextMeshProUGUI>("Head/Data/Intensity");
    reanimationChance = Get<TextMeshProUGUI>("Head/Data/Chance");
    avatarBackground = Get<Image>("Head/Avatar/Background");
    healCost = Get<TextMeshProUGUI>("WoundedHead/Value");
    woundedSlots = Get<RectTransform>("WoundedSlots");
    reanimationCost = Get<TextMeshProUGUI>("DeadHead/Value");
    deadSlots = Get<RectTransform>("DeadSlots");

    if (!ComponentsInitialized()) {
      Debug.LogError("Healing menu UI components initialization error");
      return;
    }

    InitPalette();
    InitHealValues();
    InitReanimationChances();
  }

  private bool ComponentsInitialized() {
    return title != null && level != null && avatarBackground != null &&
    intensity != null && reanimationChance != null && healCost != null &&
    woundedSlots != null && reanimationCost != null && deadSlots != null;
  }

  private void InitPalette() {
    AddColor(MasteryLevel.Novice, "#A0A0A0");
    AddColor(MasteryLevel.Apprentice, "#618C2D");
    AddColor(MasteryLevel.Adept, "#306DAB");
    AddColor(MasteryLevel.Expert, "#6948A4");
    AddColor(MasteryLevel.Master, "#CF8F0B");
  }

  private void AddColor(MasteryLevel lvl, string hex) {
    if (ColorUtility.TryParseHtmlString(hex, out var color)) palette[lvl] = color;
  }

  private void InitHealValues() {
    healValues[MasteryLevel.Novice] = new int[] { 1, 15 };
    healValues[MasteryLevel.Apprentice] = new int[] { 5, 25 };
    healValues[MasteryLevel.Adept] = new int[] { 20, 50 };
    healValues[MasteryLevel.Expert] = new int[] { 40, 100 };
    healValues[MasteryLevel.Master] = new int[] { 1000, 1000 };
  }

  private void InitReanimationChances() {
    reanimationChances[MasteryLevel.Novice] = 50;
    reanimationChances[MasteryLevel.Apprentice] = 70;
    reanimationChances[MasteryLevel.Adept] = 90;
    reanimationChances[MasteryLevel.Expert] = 100;
    reanimationChances[MasteryLevel.Master] = 100;
  }

  public void Init(string name, MasteryLevel lvl) {
    List<Unit> army = Player.Instance.Army.Units;
    if (army == null || army.Count == 0) return;
    Unit[] wounded = army.Where(u => u.CurrentHealth < u.TotalHealth && u.CurrentHealth > 0).ToArray();
    Unit[] dead = army.Where(u => u.CurrentHealth <= 0).ToArray();

    title.text = name;
    level.text = "Level: " + lvl;
    int[] intensityValues = healValues[lvl];

    intensity.text = string.Format(
      "Heal intensity: {0}-{1} points",
      intensityValues[0], intensityValues[1]
    );

    reanimationChance.text = "Reanimation chance: " + reanimationChances[lvl] + "%";
    avatarBackground.color = palette[lvl];

    if (wounded.Length > 0) {
      healCost.text = string.Format(
        "{0} unit(s) x {1}",
        wounded.Length, GetCost(baseHealCost, healCostModifier)
      );
    }
    if (dead.Length > 0) {
      reanimationCost.text = string.Format(
        "{0} unit(s) x {1}",
        dead.Length, GetCost(baseReanimationCost, reanimationCostModifier)
      );
    }

    foreach (Unit unit in wounded) {
      GameObject slot = Instantiate(slotPrefab, woundedSlots);
      slot.GetComponent<HealingMenuSlot>().Init(unit, true);
    }
    foreach (Unit unit in dead) {
      GameObject slot = Instantiate(slotPrefab, deadSlots);
      slot.GetComponent<HealingMenuSlot>().Init(unit);
    }

    RenderEmptySlots(woundedSlots, wounded.Length);
    RenderEmptySlots(deadSlots, dead.Length);
    UpdateSlotsSize(woundedSlots);
    UpdateSlotsSize(deadSlots);
  }

  public void Clear() {
    if (!ComponentsInitialized()) return;
    ClearSlots();
    title.text = "";
    level.text = "";
    healCost.text = "0";
    gameObject.SetActive(false);
  }

  private void ClearSlots() {
    foreach (Transform child in woundedSlots) Destroy(child.gameObject);
    foreach (Transform child in deadSlots) Destroy(child.gameObject);
  }

  private void UpdateSlotsSize(RectTransform slots) {
    GridLayoutGroup gridGroup = slots.GetComponent<GridLayoutGroup>();
    float totalWidth = slots.rect.width;
    float totalSpacing = slotsGap * (slotColumns - 1) + slotsGap * 2;
    float size = (totalWidth - totalSpacing) / slotColumns;
    gridGroup.cellSize = new Vector2(size, size);
  }

  private void RenderEmptySlots(RectTransform panel, int filled) {
    int placeholders = filled == 0
      ? slotColumns
      : (filled % slotColumns == 0 ? 0 : slotColumns - (filled % slotColumns));

    for (int i = 0; i < placeholders; i++) Instantiate(slotEmptyPrefab, panel);
  }

  private float GetCost(float value, float modifier) {
    int level = Player.Instance.Level;
    return Mathf.RoundToInt(value * Mathf.Pow(level, modifier));
  }
}
