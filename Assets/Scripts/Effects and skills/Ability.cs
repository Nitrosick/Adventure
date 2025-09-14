using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Ability")]
public class Ability : ScriptableObject {
  public string id;
  public string abilityName;
  [TextArea(5, 20)] public string description;
  public Sprite icon;
  public float[] effectValues = { 0, 0, 0 };
  public string effectPostfix;
}
