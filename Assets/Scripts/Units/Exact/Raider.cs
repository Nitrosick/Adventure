public class Raider : Unit
{
  private Raider() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Raider";
    Description = "A fierce raider, hardened in battle. He is ready to attack even the most protected trade caravan. Petty thefts and street brawls are not for him.";
    PrefabId = "u7";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    MaxLevel = 7;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 10;
    BehaviorType = AIBehaviorType.Aggressive;
  }
}
