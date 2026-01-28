public class Spearman : MeleeUnit {
  private Spearman() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Spearman";
    Description = "The lowest-ranking guard. They usually guard warehouses, storerooms, stables, and if they're lucky, private homes.";
    PrefabId = "u10";
    Type = UnitType.Melee;
    Level = 2;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 9;
    BehaviorType = AIBehaviorType.TryPierceHit;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Spear
    };
  }
}
