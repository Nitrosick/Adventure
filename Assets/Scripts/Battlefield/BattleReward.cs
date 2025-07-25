using System.Collections.Generic;

[System.Serializable]
public class BattleReward {
  public int experience;
  public int fame;
  public int Gold { get; private set; } = 0;
  public int[] goldRange = { 0, 0 }; // Min, Max
  public int[] resources = { 0, 0, 0, 0 }; // Wood, Stone, Metal, Leather
  public List<Equipment> equipment = new();
  public List<Item> items = new();

  public void Add(BattleReward other) {
    experience += other.experience;
    fame += other.fame;
    if (other.goldRange.Length == 2) Gold += Utils.GetRandomInRange(other.goldRange[0], other.goldRange[1]);
    for (int i = 0; i < other.resources.Length; i++) resources[i] += other.resources[i];
    equipment.AddRange(other.equipment);
    items.AddRange(other.items);
  }
}
