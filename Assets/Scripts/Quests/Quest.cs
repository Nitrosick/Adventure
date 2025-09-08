using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "GameObjects/Quests/Quest")]
public class Quest : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string description;
  [TextArea(5, 20)] public string descriptionCompleted;
  public QuestObjective objectiveType;
  public int objectiveZoneId;
  public Item objectiveItem;
  public int requiredLevel = 1;
  public Reward reward;
  public QuestState state = QuestState.Inactive;
}
