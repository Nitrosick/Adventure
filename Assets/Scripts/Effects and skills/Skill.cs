using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "GameObjects/Skill")]
public class Skill : ScriptableObject {
  public string skillName;
  [TextArea(5, 20)] public string description;
  public bool isActive = true;
  public bool canUseInRoot;
  public float activateChance = 100f;
  public int cost = 1;

  public Button skillButton;
  public BattlePhase[] skillPhases;
  public Sprite uiIcon;
  public Color uiIconColor;

  public void Trigger(Unit unit, Image icon) {
    ColorUtility.TryParseHtmlString("#4B4A47", out var defaultColor);
    ColorUtility.TryParseHtmlString("#EFBF0D", out var activeColor);

    // Switchable
    switch (skillName) {
      case "Charged attack":
        if (!unit.IsChargedAttack) {
          if (!CanUse(unit)) return;
          unit.SetSkillCharges(-cost);
          unit.IsChargedAttack = true;
          icon.color = activeColor;
        } else {
          unit.SetSkillCharges(cost);
          unit.IsChargedAttack = false;
          icon.color = defaultColor;
        }
        break;
    }

    if (!CanUse(unit)) return;

    // Usable
    switch (skillName) {
      case "Block":
        unit.SetSkillCharges(-cost);
        unit.BlockStance("e2");
        break;
      case "Wall":
        unit.SetSkillCharges(-cost);
        unit.BlockStance("e7");
        break;
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
