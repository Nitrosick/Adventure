using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "GameObjects/Quests/Quest")]
public class Quest : ScriptableObject {

  [System.Serializable]
  public class QuestZoneUpgrades {
    public string zoneId;
    public MapZoneFeature feature;
  }

  public string id;
  public string title;
  [TextArea(5, 20)] public string description;
  [TextArea(5, 20)] public string descriptionCompleted;
  public int requiredLevel = 1;
  public Reward reward;

  public QuestObjective objectiveType;
  public string objectiveZoneId;
  public Equipment[] objectiveEquipment;
  public Item[] objectiveItems;
  public QuestZoneUpgrades[] questZoneUpgrades;
}
