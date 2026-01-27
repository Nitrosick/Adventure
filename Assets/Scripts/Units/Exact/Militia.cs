public class Militia : UnitCombat
{
  private Militia() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Militia";
    Description = "Infantry often recruits strong guys who previously guarded warehouses or docks, or worked as bouncers in taverns. They are always ready for a good fight.";
    PrefabId = "u2";
    Type = UnitType.Melee;
    MaxLevel = 5;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 25f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 10;
    BehaviorType = AIBehaviorType.PriorityTarget;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon,
      EquipmentType.Spear,
      EquipmentType.PoleWeapon
    };
  }
}
