using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Leveling")]
public class LevelingItem : Item {
  public float effectValue;
  public int maxLevel;
  public UnitType unitType;
}
