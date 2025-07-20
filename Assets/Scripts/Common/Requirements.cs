// using System.Collections.Generic;

[System.Serializable]
public class Requirements {
  public int playerLevel;
  public int playerFame;
  public int gold;
  public int[] resources = { 0, 0, 0, 0 };
  public Equipment[] equipment;
  // FIXME: Обычные предметы

  // public void Add(BattleReward other) {
  //   experience += other.experience;
  //   fame += other.fame;
  //   if (other.goldRange.Length == 2) Gold += Utils.GetRandomInRange(other.goldRange[0], other.goldRange[1]);
  //   for (int i = 0; i < other.resources.Length; i++) resources[i] += other.resources[i];
  //   items.AddRange(other.items);
  // }
}
