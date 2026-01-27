public class TownGuardOfficer : UnitCombat
{
  private TownGuardOfficer() {
    Strength = 5;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Town guard officer";
    Description = "Officers command small squads of the town guard and report to captains. They are excellently trained in fencing and, though rarely participating in combat, pose a serious threat to any opponent.";
    PrefabId = "u12";
    Type = UnitType.Melee;
    Level = 3;
    MaxLevel = 8;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 40f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 7;
    Priority = 6;
    BehaviorType = AIBehaviorType.PriorityTarget;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.OneHandWeapon,
      EquipmentType.TwoHandWeapon
    };
  }
}
