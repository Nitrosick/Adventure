using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : GeneralUI {
  public static BattleUI Instance;

  // Prefabs
  public GameObject damagePopupPrefab;
  public GameObject skillChargePrefab;
  public GameObject skillChargeEmptyPrefab;

  // Panels
  private Transform queuePanel;
  private Transform actionsPanel;
  private Transform skillsPanel;
  private Transform supportsPanel;

  // Buttons and labels
  private Button phaseSkipButton;
  private Button climbButton;
  private Image phaseAttackLabel;
  private Image phaseMoveLabel;
  private TextMeshProUGUI currentRound;
  private TextMeshProUGUI reinforcement;

  private Color activeColor;
  private Color inactiveColor;

  protected override void Awake() {
    base.Awake();
    Instance = this;

    Transform top = transform.Find("Top");
    queuePanel = transform.Find("TurnQueue/Panel");
    actionsPanel = transform.Find("Actions/Panel");
    skillsPanel = transform.Find("Actions/Panel/Skills");
    supportsPanel = transform.Find("Supports/Panel");

    mainMenuButton = Get<Button>(top, "MainMenu/Main");
    currentRound = Get<TextMeshProUGUI>(top, "Panel/Round/Value");
    reinforcement = Get<TextMeshProUGUI>(top, "Panel/Round/Reinforcement");
    phaseSkipButton = Get<Button>(actionsPanel, "SkipPhase");
    climbButton = Get<Button>(actionsPanel, "Climb");
    phaseAttackLabel = Get<Image>(actionsPanel, "PhaseAttack");
    phaseMoveLabel = Get<Image>(actionsPanel, "PhaseMovement");

    if (
      queuePanel == null || actionsPanel == null || skillsPanel == null ||
      phaseSkipButton == null || phaseAttackLabel == null || phaseMoveLabel == null ||
      currentRound == null || climbButton == null || supportsPanel == null ||
      reinforcement == null
    ) {
      Debug.LogError("Battle UI components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#FFFFFF", out activeColor);
    ColorUtility.TryParseHtmlString("#989898", out inactiveColor);

    phaseSkipButton.onClick.AddListener(SkipPhase);
    climbButton.onClick.AddListener(Climb);
  }

  void Start() {
    UpdateReinforcementInfo(1);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    phaseSkipButton.onClick.RemoveListener(SkipPhase);
    climbButton.onClick.RemoveListener(Climb);
  }

  private void SkipPhase() {
    PhaseManager.NextPhase();
  }

  public override void DisableUI() {
    base.DisableUI();
    phaseSkipButton.interactable = false;
    climbButton.interactable = false;
    skillsPanel.gameObject.SetActive(false);
  }

  public override void EnableUI() {
    base.EnableUI();
    phaseSkipButton.interactable = true;
    climbButton.interactable = true;
    skillsPanel.gameObject.SetActive(true);
  }

  protected override void OpenPauseMenu() {
    AlmanacUI.Instance.Close();
    PauseMenu.Open();
  }

  protected override void OpenAlmanac() {
    AlmanacUI.Instance.Open();
  }

  public void UpdateQueue(List<Unit> queue, int current = 0) {
    foreach (Transform child in queuePanel) Destroy(child.gameObject);

    currentRound.text = $"Round {QueueManager.Instance.Round}";
    UpdateReinforcementInfo(QueueManager.Instance.Round);
    int count = queue.Count;

    for (int i = 0; i < count; i++) {
      int index = (current + i) % count;
      Unit unit = queue[index];
      if (unit.IsDead) continue;
      GameObject slot = Instantiate(GameManager.I.slotQueue, queuePanel);
      QueueSlot slotScript = slot.GetComponent<QueueSlot>();

      slotScript.Init(unit);
      if (i == 0) slotScript.SetActive();
    }
  }

  public void UpdateSupports(List<SupportInstance> supports) {
    if (supports.Count == 0) return;

    supportsPanel.gameObject.SetActive(true);
    foreach (Transform child in supportsPanel) Destroy(child.gameObject);

    foreach (SupportInstance unit in supports) {
      unit.relation = UnitRelation.Ally;
      GameObject slot = Instantiate(GameManager.I.slotQueue, supportsPanel);
      slot.GetComponent<QueueSlot>().Init(unit);
      // TODO: Саппорты противника
    }
  }

  public void SwitchPhase(BattlePhase phase) {
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

  public void ShowClimbButton() {
    climbButton.gameObject.SetActive(true);
  }

  public void HideClimbButton() {
    climbButton.gameObject.SetActive(false);
  }

  public void Climb() {
    QueueManager.Instance.CurrentUnit.Move.Climb();
  }

  public void ShowSkills(List<Skill> skills, BattlePhase phase, Unit unit) {
    foreach (Transform child in skillsPanel) Destroy(child.gameObject);

    if (unit == null) {
      Debug.LogError("Unit not found");
      return;
    }

    List<Skill> filtered = skills
      .Where(s => {
        if (unit.Effects.HasAnyEffect(new string[] { "Stun", "Root" }) && !s.canUseInRoot) return false;
        return s != null && s.skillPhases.Contains(phase);
      })
      .ToList();

    foreach (Skill skill in filtered) {
      Button button = Instantiate(skill.skillButton, skillsPanel);
      Image icon = button.transform.Find("Icon").GetComponent<Image>();
      button.onClick.AddListener(() => skill.Trigger(unit, icon));
      if (unit.SkillCharges <= 0) button.interactable = false;
    }
  }

  public void DisableSkills() {
    foreach (Transform child in skillsPanel) {
      child.GetComponent<Button>().interactable = false;
    }
  }

  public void UpdateReinforcementInfo(int round) {
    int reinforcementRound = StateManager.reinforcementRound;

    if (reinforcementRound == 0 || round >= reinforcementRound) {
      reinforcement.gameObject.SetActive(false);
      return;
    }

    reinforcement.gameObject.SetActive(true);
    int delta = reinforcementRound - round;
    reinforcement.text = $"Reinforcements will arrive in {delta} round{(delta == 1 ? "" : "s")}";
  }
}
