using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "GameObjects/Achievement")]
public class Achievement : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string description;
  public int objectiveCount;
  public float defaultProgress;
  public Reward reward;
}
