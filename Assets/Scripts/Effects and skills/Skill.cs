using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "GameObjects/Skill")]
public class Skill : ScriptableObject {
  public string skillName;
  [TextArea(5, 20)] public string description;
  public bool isActive = true;
  public bool canUseInRoot;
  public bool needTarget;
  public float activateChance = 100f;
  public int cost = 1;

  public Button skillButton;
  public BattlePhase[] skillPhases;
  public Sprite uiIcon;
  public Color uiIconColor;

  public void Trigger(Unit unit, Image icon) {
    ColorUtility.TryParseHtmlString("#4B4A47", out var defaultColor);
    ColorUtility.TryParseHtmlString("#EFBF0D", out var activeColor);

    bool handled = false;

    // Switchable
    switch (skillName) {
      case "Charged attack":
        handled = true;
        ToggleAttackType(unit, AttackType.Charged, icon, activeColor, defaultColor);
        break;
      case "Fan attack":
        handled = true;
        ToggleAttackType(unit, AttackType.Fan, icon, activeColor, defaultColor);
        break;
    }

    if (handled || !CanUse(unit)) return;

    // Usable
    switch (skillName) {
      case "Block":
        unit.SetSkillCharges(-cost);
        unit.Skills.BlockStance("e2");
        break;
      case "Wall":
        unit.SetSkillCharges(-cost);
        unit.Skills.BlockStance("e7");
        break;
    }
  }

  private void ToggleAttackType(
    Unit unit,
    AttackType targetType,
    Image icon,
    Color activeColor,
    Color defaultColor
  ) {
    bool isActive = unit.CurrentAttackType == targetType;

    if (!isActive) {
      if (!CanUse(unit)) return;
      unit.SetSkillCharges(-cost);
      unit.SetAttackType(targetType);
      icon.color = activeColor;
    } else {
      unit.SetSkillCharges(cost);
      unit.SetAttackType(AttackType.Standard);
      icon.color = defaultColor;
    }
  }



  private bool CanUse(Unit unit) {
    if (cost > unit.SkillCharges) {
      _ = Toast.Show("charge", "Not enough skill points", 2);
      return false;
    }
    return true;
  }
}
