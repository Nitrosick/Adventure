using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour {
  private Button button;
  private Image frame;
  private Shadow frameShadow;
  private Image icon;
  private Shadow iconShadow;
  private TooltipTrigger hint;
  private AbilityInstance ability;

  private void Awake() {
    button = transform.GetComponent<Button>();
    frame = transform.Find("Frame").GetComponent<Image>();
    frameShadow = transform.Find("Frame").GetComponent<Shadow>();
    icon = transform.Find("Icon").GetComponent<Image>();
    iconShadow = transform.Find("Icon").GetComponent<Shadow>();
    hint = transform.GetComponent<TooltipTrigger>();

    if (
      button == null || frame == null || icon == null ||
      hint == null || frameShadow == null || iconShadow == null
    ) {
      Debug.LogError("Ability slot components initialization error");
      return;
    }

    button.onClick.AddListener(OpenAbility);
  }

  private void OnDestroy() {
    button.onClick.RemoveListener(OpenAbility);
  }

  public void Init(AbilityInstance _ability) {
    ability = _ability;
    icon.sprite = ability.data.icon;
    frame.color = PlayerMenuAbilitiesUI.palette[ability.level];
    icon.color = PlayerMenuAbilitiesUI.palette[ability.level];
    hint.message = $"Learn: {ability.data.abilityName}";

    if (ability.level == AbilityLevel.No) {
      Color c = frame.color;
      c.a = 0.05f;
      frame.color = c;
      icon.color = c;
      frameShadow.enabled = false;
      iconShadow.enabled = false;
    }
  }

  private void OpenAbility() {
    int points = Player.Instance.AbilityPoints;
    string effect = ability.level == AbilityLevel.No
      ? ""
      : $"{ability.data.effectValues[LevelIndex(ability.level) - 1]}{ability.data.effectPostfix}";
    if (ability.level == AbilityLevel.Gold) effect += " (max.)";

    Dialog.Learn(
      LearnAbility,
      ability.data.abilityName,
      ability.data.description,
      effect,
      ability.data.icon,
      points > 0 && ability.level != AbilityLevel.Gold
    );
  }

  private void LearnAbility(bool accepted) {
    if (!accepted) return;
    AbilityController.Learn(ability.data.id);
    PlayerMenuAbilitiesUI.Init();
    PlayerMenuUIInfo.RecalculatePoints();
    _ = Toast.Show("success", "Ability learned");
  }

  private static int LevelIndex(AbilityLevel level) {
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }
}
