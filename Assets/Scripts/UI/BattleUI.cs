using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
  public static BattleUI Instance;

  // Prefabs
  [SerializeField] private GameObject queueSlotPrefab;
  public GameObject damagePopupPrefab;
  public GameObject skillChargePrefab;
  public GameObject skillChargeEmptyPrefab;

  // Panels
  private static Transform queuePanel;
  private static Transform actionsPanel;
  private static Transform skillsPanel;

  // Buttons and labels
  private static Button mainMenuButton;
  private static Button phaseSkipButton;
  private static Image phaseAttackLabel;
  private static Image phaseMoveLabel;
  private static TextMeshProUGUI currentRound;

  private static Color activeColor;
  private static Color inactiveColor;

  private void Awake() {
    Instance = this;

    Transform Find(string path) => transform.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    queuePanel = Get<Transform>("TurnQueue/Panel");
    actionsPanel = Get<Transform>("Actions/Panel");
    skillsPanel = Get<Transform>("Actions/Panel/Skills");

    mainMenuButton = Get<Button>("Top/MainMenu/Main");
    phaseSkipButton = actionsPanel.Find("SkipPhase").GetComponent<Button>();
    phaseAttackLabel = actionsPanel.Find("PhaseAttack").GetComponent<Image>();
    phaseMoveLabel = actionsPanel.Find("PhaseMovement").GetComponent<Image>();

    currentRound = Get<TextMeshProUGUI>("Top/Round/Value");

    if (
      queuePanel == null || actionsPanel == null || skillsPanel == null ||
      phaseSkipButton == null || phaseAttackLabel == null || phaseMoveLabel == null ||
      damagePopupPrefab == null || skillChargePrefab == null || skillChargeEmptyPrefab == null ||
      mainMenuButton == null || currentRound == null
    ) {
      Debug.LogError("Battle UI components initialization error");
      return;
    }

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
