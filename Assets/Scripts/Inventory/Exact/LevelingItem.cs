using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Leveling")]
public class LevelingItem : Item {
  public float effectValue;
  public int maxLevel;
  public UnitType unitType;

  public override void Use() {
    Unit[] units = Player.Instance.Army.Units
      .Where(u => u.Type == unitType && u.Level < maxLevel && u.Level < u.MaxLevel)
      .ToArray();

    if (units.Length == 0) {
      _ = Toast.Show("warning", "No suitable units");
      return;
    }

    foreach (Unit unit in units) unit.LevelUp((int)effectValue);
    _ = Toast.Show("success", "Unit levels increased");
  }
}
