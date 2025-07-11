public class Bandit : Unit
{
  private Bandit() {
    Strength = 4;
    Dexterity = 3;
    Intelligence = 1;

    Name = "Bandit";
    Description = "This is a fast, agile, but poorly protected fighter who uses swords and daggers. Bandits set up camps in forests and hunt passing travelers.";
    prefabId = "u3";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    TotalHealth = 20f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 6;
    Priority = 12;
  }
}
