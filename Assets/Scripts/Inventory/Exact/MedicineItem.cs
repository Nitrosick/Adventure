using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Medicine")]
public class MedicineItem : Item {
  public float intensity;

  public override void Use() {
    Unit[] woundedUnits = Player.Instance.Army.Units
      .Where(u => u.CurrentHealth > 0 && u.CurrentHealth < u.Health.GetMaxHP())
      .ToArray();

    if (woundedUnits.Length == 0) {
      _ = Toast.Show("warning", "No wounded units");
      return;
    }

    foreach (Unit unit in woundedUnits) unit.Health.Heal(intensity);
    _ = Toast.Show("success", "Units are cured");
  }
}
