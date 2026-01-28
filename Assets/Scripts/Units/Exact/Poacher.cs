public class Poacher : RangeUnit {
  private Poacher() {
    Strength = 2;
    Dexterity = 4;
    Intelligence = 1;

    Name = "Poacher";
    Description = "They hunt animals, especially rare ones. But when money is really scarce, they won't hesitate to join a bandit group";
    PrefabId = "u6";
    Type = UnitType.Range;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Dexterity;
    TotalHealth = 20f;
    MoveSpeed = 3.5f;
    DefaultMovePoints = 5;
    Initiative = 8;
    Priority = 16;
    Projectiles = 25;
    BehaviorType = AIBehaviorType.KeepDistance;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Bow
    };
  }
}
