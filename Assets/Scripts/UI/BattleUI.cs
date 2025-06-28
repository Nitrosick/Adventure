using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
  public static BattleUI Instance;
  [SerializeField] private GameObject avatarPrefab;
  public GameObject damagePopupPrefab;

  // Panels
  private static Transform queuePanel;
  private static Transform actionsPanel;
  private static Transform skillsPanel;
  private static GameObject unitInfoPanel;

  // Fields
  private static TextMeshProUGUI unitName;
  private static TextMeshProUGUI unitDescription;
  private static TextMeshProUGUI unitHP;
  private static TextMeshProUGUI unitStats;
  private static TextMeshProUGUI unitMP;
  private static TextMeshProUGUI unitDamage;
  private static TextMeshProUGUI unitDefense;
  private static TextMeshProUGUI unitRange;
  private static TextMeshProUGUI unitEffects;

  // Buttons and labels
  private static Button phaseSkipButton;
  private static Image phaseAttackLabel;
  private static Image phaseMoveLabel;

  private static Color allyColor;
  private static Color enemyColor;
  private static Color activeColor;
  private static Color inactiveColor;

  private void Awake() {
    Instance = this;
    queuePanel = transform.Find("TurnQueue/Panel").GetComponent<Transform>();
    actionsPanel = transform.Find("Actions/Panel").GetComponent<Transform>();
    skillsPanel = transform.Find("Actions/Panel/Skills").GetComponent<Transform>();
    unitInfoPanel = transform.Find("Info/UnitInfoPanel").gameObject;

    unitName = transform.Find("Info/UnitInfoPanel/Name").GetComponent<TextMeshProUGUI>();
    unitDescription = transform.Find("Info/UnitInfoPanel/Description").GetComponent<TextMeshProUGUI>();
    Transform parameters = transform.Find("Info/UnitInfoPanel/Parameters").GetComponent<Transform>();
    unitHP = parameters.Find("HP/Value").GetComponent<TextMeshProUGUI>();
    unitStats = parameters.Find("Stats/Value").GetComponent<TextMeshProUGUI>();
    unitMP = parameters.Find("MP/Value").GetComponent<TextMeshProUGUI>();
    unitDamage = parameters.Find("Damage/Value").GetComponent<TextMeshProUGUI>();
    unitDefense = parameters.Find("Defense/Value").GetComponent<TextMeshProUGUI>();
    unitRange = parameters.Find("Range/Value").GetComponent<TextMeshProUGUI>();
    unitEffects = transform.Find("Info/UnitInfoPanel/Effects").GetComponent<TextMeshProUGUI>();

    phaseSkipButton = actionsPanel.Find("SkipPhase").GetComponent<Button>();
    phaseAttackLabel = actionsPanel.Find("PhaseAttack").GetComponent<Image>();
    phaseMoveLabel = actionsPanel.Find("PhaseMovement").GetComponent<Image>();

    if (
      queuePanel == null || actionsPanel == null || skillsPanel == null || unitInfoPanel == null ||
      unitName == null || unitDescription == null || unitHP == null || unitStats == null ||
      unitMP == null || unitDamage == null || unitDefense == null || unitRange == null ||
      unitEffects == null || phaseSkipButton == null || phaseAttackLabel == null || phaseMoveLabel == null
    ) {
      Debug.LogError("Battle UI components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#174E87", out allyColor);
    ColorUtility.TryParseHtmlString("#781010", out enemyColor);
    ColorUtility.TryParseHtmlString("#FFFFFF", out activeColor);
    ColorUtility.TryParseHtmlString("#989898", out inactiveColor);

    phaseSkipButton.onClick.AddListener(SkipPhase);
  }

  private void OnDestroy() {
    phaseSkipButton.onClick.RemoveListener(SkipPhase);
  }

  private void SkipPhase() {
    PhaseManager.NextPhase();
  }

  public static void DisableUI() {
    phaseSkipButton.interactable = false;
    skillsPanel.gameObject.SetActive(false);
  }

  public static void EnableUI() {
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

  public static void ShowUnitInfo(string name, string desc, float[] stats, float mp, float totalHp, float hp, int damage, float def, int range, List<EffectInstance> effects) {
    unitInfoPanel.SetActive(true);
    unitName.text = name;
    unitDescription.text = desc;
    unitHP.text = string.Format(
      "{0} / {1}",
      totalHp / 3 > hp ? "<color=#F61010>" + Math.Ceiling(hp).ToString() + "</color>" : Math.Ceiling(hp).ToString(),
      totalHp.ToString()
    );
    unitStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      stats[0],
      stats[1],
      stats[2]
    );
    unitMP.text = mp.ToString();
    unitDamage.text = damage.ToString();
    unitDefense.text = def.ToString();
    unitRange.text = range.ToString();

    string effectsText = "Effects";
    foreach (EffectInstance effect in effects) {
      if (effect.effectData.isNegative) effectsText += "\n<color=#F61010>" + effect.effectData.effectName + "</color>";
      else effectsText += "\n<color=#81D11F>" + effect.effectData.effectName + "</color>";
    }
    unitEffects.text = effectsText;
  }

  public static void HideUnitInfo() {
    unitInfoPanel.SetActive(false);
    unitName.text = "";
    unitDescription.text = "";
    unitHP.text = "";
    unitStats.text = "";
    unitMP.text = "";
    unitDamage.text = "";
    unitDefense.text = "";
    unitRange.text = "";
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
