using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Ability")]
public class Ability : ScriptableObject {
  public string id;
  public string abilityName;
  [TextArea(5, 20)] public string description;
  public float[] effectValues = { 0, 0, 0 };
  public string effectPostfix;
  public int tier = 1;

  public Sprite icon;
  public AbilityBonusType bonusType;
}
