public class Thief : UnitCombat
{
  private Thief() {
    Strength = 1;
    Dexterity = 4;
    Intelligence = 1;

    Name = "Thief";
    Description = "An elusive and fast melee unit. Excellent with a dagger and stealth skills. A very dangerous opponent, especially at night";
    PrefabId = "u13";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.Dagger;
    MaxLevel = 7;
    LevelingCoreStat = CoreStat.Dexterity;
    TotalHealth = 25f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 8;
    Priority = 13;
    BehaviorType = AIBehaviorType.PriorityTarget;
  }
}
