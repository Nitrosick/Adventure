using System.Collections.Generic;

[System.Serializable]
public class Reward {
  public int experience;
  public int fame;
  public int reputation;
  public int statPoints;
  public int abilityPoints;
  public int villagers;
  public int[] goldRange = { 0, 0 }; // Min, Max Gold
  public int[] resources = { 0, 0, 0, 0 }; // Wood, Stone, Metal, Leather
  public List<Equipment> equipment = new();
  public List<Item> items = new();
  public int projectiles;

  public void Add(Reward other) {
    experience += other.experience;
    fame += other.fame;
    reputation += other.reputation;
    statPoints += other.statPoints;
    abilityPoints += other.abilityPoints;
    villagers += other.villagers;

    for (int i = 0; i < other.goldRange.Length; i++) goldRange[i] += other.goldRange[i];
    for (int i = 0; i < other.resources.Length; i++) resources[i] += other.resources[i];

    equipment.AddRange(other.equipment);
    items.AddRange(other.items);
    foreach (Equipment item in equipment) item.isNew = true;
    foreach (Item item in items) item.isNew = true;
  }
}
