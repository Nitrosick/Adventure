public class HeroWarrior : MeleeUnit {
  private HeroWarrior() {
    Strength = 5;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Hero";
    Description = "The fate of this warrior is entirely in your hands.";
    IsHero = true;
    PrefabId = "u1";
    Type = UnitType.Melee;
    MaxLevel = 30;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 40f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 7;
    Priority = 6;
    BehaviorType = AIBehaviorType.Passive;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon,
      EquipmentType.Spear,
      EquipmentType.PoleWeapon
    };
  }
}
