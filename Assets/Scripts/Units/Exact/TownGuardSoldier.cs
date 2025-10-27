public class TownGuardSoldier : UnitCombat {
  private TownGuardSoldier() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Town guard soldier";
    Description = "A town guard fighter who has proven himself in battle. Skillfully wields a one-handed weapon and shield. Ready to give his life for his kingdom.";
    PrefabId = "u11";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    Level = 2;
    MaxLevel = 7;
    LevelingCoreStat = CoreStat.Strength;
    ShieldIsAllow = true;
    TotalHealth = 32f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 8;
    BehaviorType = AIBehaviorType.PriorityTarget;
  }
}
