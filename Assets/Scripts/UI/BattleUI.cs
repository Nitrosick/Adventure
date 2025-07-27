using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
  public static BattleUI Instance;

  // Prefabs
  [SerializeField] private GameObject avatarPrefab;
  public GameObject damagePopupPrefab;
  public GameObject skillChargePrefab;
  public GameObject skillChargeEmptyPrefab;

  // Panels
  private static Transform queuePanel;
  private static Transform actionsPanel;
  private static Transform skillsPanel;
  private static GameObject unitInfoPanel;

  // Fields
  private static TextMeshProUGUI unitName;
  private static TextMeshProUGUI unitDescription;
  private static TextMeshProUGUI unitHP;
  private static TextMeshProUGUI unitLevel;
  private static TextMeshProUGUI unitStats;
  private static TextMeshProUGUI unitMP;
  private static TextMeshProUGUI unitDamage;
  private static TextMeshProUGUI unitDefense;
  private static TextMeshProUGUI unitRange;
  private static TextMeshProUGUI unitSkillCharges;
  private static TextMeshProUGUI unitEffects;

  // Buttons and labels
  private static Button mainMenuButton;
  private static Button phaseSkipButton;
  private static Image phaseAttackLabel;
  private static Image phaseMoveLabel;

  private static Color allyColor;
  private static Color enemyColor;
  private static Color activeColor;
  private static Color inactiveColor;

  private void Awake() {
    Instance = this;

    Transform Find(string path) => transform.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    queuePanel = Get<Transform>("TurnQueue/Panel");
    actionsPanel = Get<Transform>("Actions/Panel");
    skillsPanel = Get<Transform>("Actions/Panel/Skills");
    unitInfoPanel = Find("Info/UnitInfoPanel").gameObject;

    unitName = Get<TextMeshProUGUI>("Info/UnitInfoPanel/Name");
    unitDescription = Get<TextMeshProUGUI>("Info/UnitInfoPanel/Description");

    Transform parameters = transform.Find("Info/UnitInfoPanel/Parameters").GetComponent<Transform>();
    T GetInParams<T>(string path) where T : Component => parameters.Find(path).GetComponent<T>();

    unitHP = GetInParams<TextMeshProUGUI>("HP/Value");
    unitLevel = GetInParams<TextMeshProUGUI>("Level/Value");
    unitStats = GetInParams<TextMeshProUGUI>("Stats/Value");
    unitMP = GetInParams<TextMeshProUGUI>("MP/Value");
    unitDamage = GetInParams<TextMeshProUGUI>("Damage/Value");
    unitDefense = GetInParams<TextMeshProUGUI>("Defense/Value");
    unitRange = GetInParams<TextMeshProUGUI>("Range/Value");
    unitSkillCharges = GetInParams<TextMeshProUGUI>("SkillCharges/Value");
    unitEffects = GetInParams<TextMeshProUGUI>("Info/UnitInfoPanel/Effects");

    mainMenuButton = Get<Button>("Top/MainMenu/Main");
    phaseSkipButton = actionsPanel.Find("SkipPhase").GetComponent<Button>();
    phaseAttackLabel = actionsPanel.Find("PhaseAttack").GetComponent<Image>();
    phaseMoveLabel = actionsPanel.Find("PhaseMovement").GetComponent<Image>();

    if (
      queuePanel == null || actionsPanel == null || skillsPanel == null || unitInfoPanel == null ||
      unitName == null || unitDescription == null || unitHP == null || unitStats == null ||
      unitMP == null || unitDamage == null || unitDefense == null || unitRange == null ||
      unitEffects == null || phaseSkipButton == null || phaseAttackLabel == null || phaseMoveLabel == null ||
      damagePopupPrefab == null || skillChargePrefab == null || skillChargeEmptyPrefab == null ||
      mainMenuButton == null || unitLevel == null || unitSkillCharges == null
    ) {
      Debug.LogError("Battle UI components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#174E87", out allyColor);
    ColorUtility.TryParseHtmlString("#781010", out enemyColor);
    ColorUtility.TryParseHtmlString("#FFFFFF", out activeColor);
    ColorUtility.TryParseHtmlString("#989898", out inactiveColor);

    mainMenuButton.onClick.AddListener(() => PauseMenu.Open());
    phaseSkipButton.onClick.AddListener(SkipPhase);
  }

  private void OnDestroy() {
    mainMenuButton.onClick.RemoveListener(() => PauseMenu.Open());
    phaseSkipButton.onClick.RemoveListener(SkipPhase);
  }

  private void SkipPhase() {
    PhaseManager.NextPhase();
  }

  public static void DisableUI() {
    mainMenuButton.interactable = false;
    phaseSkipButton.interactable = false;
    skillsPanel.gameObject.SetActive(false);
  }

  public static void EnableUI() {
    mainMenuButton.interactable = true;
    phaseSkipButton.interactable = true;
    skillsPanel.gameObject.SetActive(true);
  }

  public static void UpdateQueue(List<Unit> queue, int current = 0) {
    foreach (Transform child in queuePanel) {
      Destroy(child.gameObject);
    }

    int count = queue.Count;

    for (int i = 0; i < count; i++) {
      int index = (current + i) % count;
      Unit unit = queue[index];
      if (unit.IsDead) continue;
      GameObject avatar = Instantiate(Instance.avatarPrefab, queuePanel);

      if (i == 0) {
        GameObject activeFrame = avatar.transform.Find("FrameActive").gameObject;
        activeFrame.SetActive(true);
      }

      Image indicator = avatar.transform.Find("RelationIndicator").GetComponent<Image>();
      Color color = unit.Relation == UnitRelation.Ally ? allyColor : enemyColor;
      indicator.color = color;

      if (unit.IsHero) {
        GameObject crown = avatar.transform.Find("Crown").gameObject;
        crown.SetActive(true);
      }

      if (unit.avatar == null) continue;
      Image portrait = avatar.transform.Find("Portrait").GetComponent<Image>();
      portrait.sprite = unit.avatar;
    }
  }

  public static void SwitchPhase(BattlePhase phase) {
    switch (phase) {
      case BattlePhase.Movement:
        phaseMoveLabel.color = activeColor;
        phaseAttackLabel.color = inactiveColor;
        break;
      case BattlePhase.Attack:
        phaseMoveLabel.color = inactiveColor;
        phaseAttackLabel.color = activeColor;
        break;
    }
  }

  public static void ShowUnitInfo(Unit unit) {
    unitInfoPanel.SetActive(true);
    unitName.text = unit.Name;
    unitDescription.text = unit.Description;
    unitHP.text = string.Format(
      "{0} / {1}",
      unit.TotalHealth / 3 > unit.CurrentHealth ? "<color=#F61010>" + Math.Ceiling(unit.CurrentHealth) + "</color>" : Math.Ceiling(unit.CurrentHealth),
      unit.TotalHealth
    );
    unitLevel.text = unit.Level.ToString();
    unitStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      unit.Strength, unit.Dexterity, unit.Intelligence
    );
    unitMP.text = unit.TotalMovePoints.ToString();
    unitDamage.text = unit.Equip.primaryWeapon.damage.ToString();
    unitDefense.text = unit.Equip.GetTotalDefense().ToString();
    unitRange.text = unit.Equip.primaryWeapon.range.ToString();
    unitSkillCharges.text = unit.SkillCharges.ToString();

    string effectsText = "Effects";
    foreach (EffectInstance e in unit.Effects.ActiveEffects) {
      if (e.effectData.isNegative) effectsText += "\n<color=#F61010>" + e.effectData.effectName + "</color>";
      else effectsText += "\n<color=#81D11F>" + e.effectData.effectName + "</color>";
    }
    unitEffects.text = effectsText;
  }

  public static void HideUnitInfo() {
    unitInfoPanel.SetActive(false);
    unitName.text = "";
    unitDescription.text = "";
    unitHP.text = "";
    unitLevel.text = "";
    unitStats.text = "";
    unitMP.text = "";
    unitDamage.text = "";
    unitDefense.text = "";
    unitRange.text = "";
    unitSkillCharges.text = "";
    unitEffects.text = "Effects";
  }

  public static void ShowSkills(List<Skill> skills, BattlePhase phase, Unit unit) {
    foreach (Transform child in skillsPanel) {
      Destroy(child.gameObject);
    }

    if (unit == null) {
      Debug.LogError("Unit not found");
      return;
    }

    List<Skill> filtered = skills
      .Where(s => s != null && s.skillPhases.Contains(phase))
      .ToList();

    foreach (Skill skill in filtered) {
      Button button = Instantiate(skill.skillButton, skillsPanel);
      button.onClick.AddListener(() => skill.Apply(unit));
      if (unit.SkillCharges <= 0) button.interactable = false;
    }
  }

  public static void DisableSkills() {
    foreach (Transform child in skillsPanel) {
      child.GetComponent<Button>().interactable = false;
    }
  }
}
