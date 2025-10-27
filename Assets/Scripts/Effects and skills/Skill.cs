using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "GameObjects/Skill")]
public class Skill : ScriptableObject {
  public string displayName;
  [TextArea(5, 20)] public string description;
  public bool isActive = true;
  public bool canUseInRoot;
  public float activateChance = 100f;

  public SkillName skillName;
  public Button skillButton;
  public BattlePhase[] skillPhases;
  public Sprite uiIcon;
  public Color uiIconColor;

  public void Trigger(Unit unit, Image icon) {
    ColorUtility.TryParseHtmlString("#4B4A47", out var defaultColor);
    ColorUtility.TryParseHtmlString("#EFBF0D", out var activeColor);

    switch (skillName) {
      case SkillName.Block:
      case SkillName.Wall:
        unit.BlockStance(skillName);
        break;
      case SkillName.ChargedAttack:
        unit.IsChargedAttack = !unit.IsChargedAttack;
        icon.color = unit.IsChargedAttack ? activeColor : defaultColor;
        break;
    }
  }
}
