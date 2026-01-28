public class TownGuardSoldier : MeleeUnit {
  private TownGuardSoldier() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Town guard soldier";
    Description = "A town guard fighter who has proven himself in battle. Skillfully wields a one-handed weapon and shield. Ready to give his life for his kingdom.";
    PrefabId = "u11";
    Type = UnitType.Melee;
    Level = 2;
    MaxLevel = 7;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 32f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 8;
    BehaviorType = AIBehaviorType.PriorityTarget;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon,
      EquipmentType.Spear,
      EquipmentType.PoleWeapon
    };
  }
}
