using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Skill")]
public class Skill : ScriptableObject {
  public string displayName;

  public SkillName skillName;
  public Button skillButton;
  public BattlePhase[] skillPhases;

  public void Apply(Unit unit) {
    switch (skillName) {
      case SkillName.Block:
        unit.Block();
        break;
    }
  }
}
