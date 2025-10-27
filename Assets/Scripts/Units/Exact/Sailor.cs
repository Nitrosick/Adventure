public class Sailor : UnitCombat {
  private Sailor() {
    Strength = 4;
    Dexterity = 2;
    Intelligence = 1;

    Name = "Sailor";
    Description = "A sailor's life is spent on a ship's deck, and the sea is harsh. That's why sailors are hardened warriors, until they drink themselves to death.";
    PrefabId = "u8";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    MaxLevel = 4;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 22f;
    MoveSpeed = 3.3f;
    DefaultMovePoints = 5;
    Initiative = 6;
    Priority = 11;
    BehaviorType = AIBehaviorType.Aggressive;
  }
}
