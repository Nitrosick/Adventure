using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour {
  private Button button;
  private Image frame;
  private Shadow frameShadow;
  private Image icon;
  private TextMeshProUGUI tier;
  private Shadow iconShadow;
  private TooltipTrigger hint;
  private AbilityInstance ability;

  void Awake() {
    button = transform.GetComponent<Button>();
    frame = transform.Find("Frame").GetComponent<Image>();
    frameShadow = transform.Find("Frame").GetComponent<Shadow>();
    icon = transform.Find("Icon").GetComponent<Image>();
    tier = transform.Find("Tier").GetComponent<TextMeshProUGUI>();
    iconShadow = transform.Find("Icon").GetComponent<Shadow>();
    hint = transform.GetComponent<TooltipTrigger>();

    if (
      button == null || frame == null || icon == null ||
      hint == null || frameShadow == null || iconShadow == null ||
      tier == null
    ) {
      Debug.LogError("Ability slot components initialization error");
      return;
    }

    button.onClick.AddListener(OpenAbility);
  }

  void OnDestroy() {
    button.onClick.RemoveListener(OpenAbility);
  }

  public void Init(AbilityInstance _ability) {
    ability = _ability;
    icon.sprite = ability.data.icon;
    frame.color = PlayerMenuUIAbilities.palette[ability.level];
    icon.color = PlayerMenuUIAbilities.palette[ability.level];
    hint.message = $"Learn: {ability.data.abilityName}";

    tier.text = ability.data.tier switch {
      1 => "I",
      2 => "II",
      3 => "III",
      _ => "-",
    };

    if (ability.level == AbilityLevel.No) {
      Color c = frame.color;
      c.a = 0.1f;
      frame.color = c;
      icon.color = c;
      frameShadow.enabled = false;
      iconShadow.enabled = false;
    }
  }

  private void OpenAbility() {
    int points = Player.Instance.AbilityPoints;
    int playerLevel = Player.Instance.Level;
    int tier = ability.data.tier;

    string effect = ability.level == AbilityLevel.No
      ? ""
      : $"{ability.data.effectValues[LevelIndex(ability.level) - 1]}{ability.data.effectPostfix}";
    if (ability.level == AbilityLevel.Gold) effect += " (max.)";

    bool inactive = (tier == 3 && playerLevel < 20) ||
      (tier == 2 && playerLevel < 10) ||
      points == 0 ||
      ability.level == AbilityLevel.Gold;

    string warning = "";
    if (tier == 3 && playerLevel < 20) warning = "Requires hero level 20";
    else if (tier == 2 && playerLevel < 10) warning = "Requires hero level 10";
    else if (points == 0) warning = "Not enough ability points";

    Dialog.Instance.Learn(
      LearnAbility,
      ability.data.abilityName,
      ability.data.description,
      effect,
      ability.data.icon,
      ability.data.tier,
      !inactive,
      warning
    );
  }

  private void LearnAbility(bool accepted) {
    if (!accepted) return;
    AbilityController.Learn(ability.data.id);
    PlayerMenuUIAbilities.Init();
    PlayerMenuUIInfo.RecalculatePoints();
    _ = Toast.Show("success", "Ability learned");
  }

  private static int LevelIndex(AbilityLevel level) {
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }
}
