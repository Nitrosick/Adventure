using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Leveling")]
public class LevelingItem : Item {
  public float effectValue;
  public int maxLevel;
  public UnitType[] allowedUnitTypes;
  public EquipmentType[] allowedEquipment;

  public override void Use() {
    switch (id) {
      case "li2":
        // TODO: Использование на алтаре
        _ = Toast.Show("warning", "It can't be applied here");
        return;
    }

    Unit[] units = Player.Instance.Army.Units
      .Where(u =>
        !u.IsHero &&
        (allowedUnitTypes.Length == 0 || allowedUnitTypes.Contains(u.Type)) &&
        u.Equip.primary != null &&
        (allowedEquipment.Length == 0 || allowedEquipment.Contains(u.Equip.primary.type)) &&
        u.Level < maxLevel &&
        u.Level < u.MaxLevel
      )
      .ToArray();

    if (units.Length == 0) {
      _ = Toast.Show("warning", "No suitable units");
      return;
    }

    foreach (Unit unit in units) unit.LevelUp((int)effectValue);
    _ = Toast.Show("success", "Unit levels increased");
  }
}
