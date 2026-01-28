public class Bully : MeleeUnit {
  private Bully() {
    Strength = 6;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Bully";
    Description = "A mountain of muscle, no thoughts, and blind obedience to orders - the ideal fighter. Can swing a heavy two-handed weapon as if holding a toothpick.";
    PrefabId = "u14";
    Type = UnitType.Melee;
    Size = ArmorSize.L;
    Level = 2;
    MaxLevel = 10;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 50f;
    MoveSpeed = 2.8f;
    DefaultMovePoints = 5;
    Initiative = 6;
    Priority = 4;
    BehaviorType = AIBehaviorType.Aggressive;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon,
      EquipmentType.Spear,
      EquipmentType.PoleWeapon
    };
  }
}
