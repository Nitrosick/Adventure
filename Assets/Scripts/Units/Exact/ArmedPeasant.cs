public class ArmedPeasant : UnitCombat
{
  private ArmedPeasant() {
    Strength = 3;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Armed peasant";
    Description = "Just yesterday, a peasant working on the farm, today already on the front line. He has no combat experience, but he can be useful in the squad.";
    PrefabId = "u5";
    Type = UnitType.Melee;
    MaxLevel = 4;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 20f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 4;
    Priority = 12;
    BehaviorType = AIBehaviorType.Aggressive;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon,
      EquipmentType.Spear,
      EquipmentType.PoleWeapon
    };
  }
}
