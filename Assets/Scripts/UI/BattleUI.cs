using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : GeneralUI {
  public static BattleUI Instance;

  // Prefabs
  [SerializeField] private GameObject queueSlotPrefab;
  public GameObject damagePopupPrefab;
  public GameObject skillChargePrefab;
  public GameObject skillChargeEmptyPrefab;

  // Panels
  private Transform queuePanel;
  private Transform actionsPanel;
  private Transform skillsPanel;

  // Buttons and labels
  private Button phaseSkipButton;
  private Button climbButton;
  private Image phaseAttackLabel;
  private Image phaseMoveLabel;
  private TextMeshProUGUI currentRound;

  private Color activeColor;
  private Color inactiveColor;

  protected override void Awake() {
    base.Awake();
    Instance = this;

    Transform top = transform.Find("Top");
    queuePanel = transform.Find("TurnQueue/Panel");
    actionsPanel = transform.Find("Actions/Panel");
    skillsPanel = transform.Find("Actions/Panel/Skills");

    mainMenuButton = Get<Button>(top, "MainMenu/Main");
    currentRound = Get<TextMeshProUGUI>(top, "Round/Value");
    phaseSkipButton = Get<Button>(actionsPanel, "SkipPhase");
    climbButton = Get<Button>(actionsPanel, "Climb");
    phaseAttackLabel = Get<Image>(actionsPanel, "PhaseAttack");
    phaseMoveLabel = Get<Image>(actionsPanel, "PhaseMovement");

    if (
      queuePanel == null || actionsPanel == null || skillsPanel == null ||
      phaseSkipButton == null || phaseAttackLabel == null || phaseMoveLabel == null ||
      currentRound == null || climbButton == null
    ) {
      Debug.LogError("Battle UI components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#FFFFFF", out activeColor);
    ColorUtility.TryParseHtmlString("#989898", out inactiveColor);

    phaseSkipButton.onClick.AddListener(SkipPhase);
    climbButton.onClick.AddListener(Climb);
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
    AlmanacUI.Close();
    PauseMenu.Open();
  }

  protected override void OpenAlmanac() {
    AlmanacUI.Open();
  }

  public void UpdateQueue(List<Unit> queue, int current = 0) {
    foreach (Transform child in queuePanel) {
      Destroy(child.gameObject);
    }

    currentRound.text = $"Round {QueueManager.Round}";
    int count = queue.Count;

    for (int i = 0; i < count; i++) {
      int index = (current + i) % count;
      Unit unit = queue[index];
      if (unit.IsDead) continue;
      GameObject slot = Instantiate(Instance.queueSlotPrefab, queuePanel);
      QueueSlot slotScript = slot.GetComponent<QueueSlot>();

      slotScript.Init(unit);
      if (i == 0) slotScript.SetActive();
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
    QueueManager.CurrentUnit.Move.Climb();
  }

  public void ShowSkills(List<Skill> skills, BattlePhase phase, Unit unit) {
    foreach (Transform child in skillsPanel) Destroy(child.gameObject);

    if (unit == null) {
      Debug.LogError("Unit not found");
      return;
    }

    List<Skill> filtered = skills
      .Where(s => {
        if ((unit.Effects.HasEffect("Stun") || unit.Effects.HasEffect("Root")) && !s.canUseInRoot) return false;
        return s != null && s.skillPhases.Contains(phase);
      })
      .ToList();

    foreach (Skill skill in filtered) {
      Button button = Instantiate(skill.skillButton, skillsPanel);
      button.onClick.AddListener(() => skill.Apply(unit));
      if (unit.SkillCharges <= 0) button.interactable = false;
    }
  }

  public void DisableSkills() {
    foreach (Transform child in skillsPanel) {
      child.GetComponent<Button>().interactable = false;
    }
  }
}
