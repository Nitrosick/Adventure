using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "GameObjects/Achievement")]
public class Achievement : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string description;
  public AchievementObjective objectiveType;
  // FIXME: Юниты или предметы (триггеры у разных объектов)
  public int objectiveCount;
  public Reward reward;
}
