using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealingMenuUI : HomeMenuFeature {
  private TextMeshProUGUI intensity;
  private TextMeshProUGUI reanimationChance;
  private TextMeshProUGUI healCost;
  private RectTransform woundedSlots;
  private TextMeshProUGUI reanimationCost;
  private RectTransform deadSlots;

  private Button healButton;
  private Button reanimateButton;

  private readonly Dictionary<MasteryLevel, int[]> healValues = new();
  private readonly Dictionary<MasteryLevel, int> reanimationChances = new();

  private readonly int baseHealCost = 3;
  private readonly float healCostModifier = 1.4f;
  private readonly int baseReanimationCost = 6;
  private readonly float reanimationCostModifier = 1.3f;

  private int woundedTotal;
  private int deadTotal;
  private List<Unit> playerArmy;
  private Unit[] wounded;
  private Unit[] dead;

  protected override void Awake() {
    base.Awake();

    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    intensity = Get<TextMeshProUGUI>("Head/Data/Intensity");
    reanimationChance = Get<TextMeshProUGUI>("Head/Data/Chance");
    healCost = Get<TextMeshProUGUI>("WoundedHead/Value");
    woundedSlots = Get<RectTransform>("WoundedSlots");
    reanimationCost = Get<TextMeshProUGUI>("DeadHead/Value");
    deadSlots = Get<RectTransform>("DeadSlots");
    healButton = Get<Button>("WoundedControl/Heal");
    reanimateButton = Get<Button>("DeadControl/Reanimate");

    if (!ComponentsInitialized()) {
      Debug.LogError("Healing menu UI components initialization error");
      return;
    }

    InitHealValues();
    InitReanimationChances();

    healButton.onClick.AddListener(HealUnits);
    reanimateButton.onClick.AddListener(ReanimateUnits);
  }

  private bool ComponentsInitialized() {
    return intensity != null && reanimationChance != null && healCost != null &&
    woundedSlots != null && reanimationCost != null && deadSlots != null &&
    healButton != null && reanimateButton != null;
  }

  private void OnDestroy() {
    healButton.onClick.RemoveListener(HealUnits);
    reanimateButton.onClick.RemoveListener(ReanimateUnits);
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
    InitHeader(name, lvl);

    int[] intensityValues = healValues[lvl];

    intensity.text = string.Format(
      "Heal intensity: {0}-{1} points",
      intensityValues[0], intensityValues[1]
    );

    reanimationChance.text = "Reanimation chance: " + reanimationChances[lvl] + "%";

    UpdateSlotsSize(woundedSlots);
    UpdateSlotsSize(deadSlots);
    UpdateUnitsData();
  }

  private void UpdateUnitsData() {
    ClearSlots(woundedSlots);
    ClearSlots(deadSlots);

    woundedTotal = 0;
    deadTotal = 0;
    healButton.interactable = false;
    reanimateButton.interactable = false;
    healCost.text = "0";
    reanimationCost.text = "0";

    playerArmy = Player.Instance.Army.Units;
    int balance = Player.Instance.Gold;
    if (playerArmy == null || playerArmy.Count == 0) return;
    wounded = playerArmy.Where(u => u.CurrentHealth < u.TotalHealth && u.CurrentHealth > 0).ToArray();
    dead = playerArmy.Where(u => u.CurrentHealth <= 0).ToArray();

    if (wounded.Length > 0) {
      int woundedCost = GetCost(baseHealCost, healCostModifier);
      woundedTotal = wounded.Length * woundedCost;
      string totalText = balance >= woundedTotal ? woundedTotal.ToString() : "<color=#F61010>" + woundedTotal + "</color>";

      healCost.text = string.Format(
        "{0} unit(s) x {1} = {2}",
        wounded.Length, woundedCost, totalText
      );

      foreach (Unit unit in wounded) {
        GameObject slot = Instantiate(slotPrefab, woundedSlots);
        slot.GetComponent<HealingMenuSlot>().Init(unit, true);
      }

      healButton.interactable = balance >= woundedTotal;
    }

    if (dead.Length > 0) {
      int deadCost = GetCost(baseReanimationCost, reanimationCostModifier);
      deadTotal = dead.Length * deadCost;
      string totalText = balance >= deadTotal ? deadTotal.ToString() : "<color=#F61010>" + deadTotal + "</color>";

      reanimationCost.text = string.Format(
        "{0} unit(s) x {1} = {2}",
        dead.Length, deadCost, totalText
      );

      foreach (Unit unit in dead) {
        GameObject slot = Instantiate(slotPrefab, deadSlots);
        slot.GetComponent<HealingMenuSlot>().Init(unit);
      }

      reanimateButton.interactable = balance >= deadTotal;
    }

    RenderEmptySlots(woundedSlots, wounded.Length);
    RenderEmptySlots(deadSlots, dead.Length);
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(woundedSlots);
    ClearSlots(deadSlots);

    healCost.text = "0";
    woundedTotal = 0;
    deadTotal = 0;
    playerArmy = null;
    wounded = null;
    dead = null;

    healButton.interactable = false;
    reanimateButton.interactable = false;
  }

  private int GetCost(int value, float modifier) {
    int level = Player.Instance.Level;
    return Mathf.RoundToInt(value * Mathf.Pow(level, modifier));
  }

  private void HealUnits() {
    foreach (Unit unit in wounded) {
      int[] intensity = healValues[masteryLevel];
      int value = Utils.GetRandomInRange(intensity[0], intensity[1]);
      unit.Heal(value, false);
    }
    Player.Instance.SetGold(woundedTotal * -1);
    UpdateUnitsData();
    StateManager.SaveGame();
  }

  private void ReanimateUnits() {
    foreach (Unit unit in dead) {
      bool success = Utils.RollChance(reanimationChances[masteryLevel]);
      if (!success) _ = InfoPopup.Show("warning", "Unit couldn't be reanimated");
      else unit.Heal(5, false);
    }
    Player.Instance.SetGold(deadTotal * -1);
    UpdateUnitsData();
    StateManager.SaveGame();
  }
}
