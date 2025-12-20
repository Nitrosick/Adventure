using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Buff")]
public class Buff : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string description;
  public Sprite icon;
}
