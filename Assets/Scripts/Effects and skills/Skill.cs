using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "GameObjects/Skill")]
public class Skill : ScriptableObject {
  public string displayName;
  [TextArea(5, 20)] public string description;
  public bool isActive = true;
  public float activateChance = 100f;

  public SkillName skillName;
  public Button skillButton;
  public BattlePhase[] skillPhases;
  public Sprite uiIcon;
  public Color uiIconColor;

  public void Apply(Unit unit) {
    switch (skillName) {
      case SkillName.Block:
      case SkillName.Wall:
        unit.BlockStance(skillName);
        break;
    }
  }
}
