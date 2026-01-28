public class Crossbowman : RangeUnit {
  private Crossbowman() {
    Strength = 3;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Crossbowman";
    Description = "Extremely immobile, well-protected shooter. Effective at medium ranges.";
    PrefabId = "u4";
    Type = UnitType.Range;
    Level = 2;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 20f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 4;
    Priority = 15;
    Projectiles = 25;
    BehaviorType = AIBehaviorType.KeepDistance;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Crossbow
    };
  }
}
