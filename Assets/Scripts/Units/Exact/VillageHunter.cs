public class VillageHunter : RangeUnit {
  private VillageHunter() {
    Strength = 2;
    Dexterity = 4;
    Intelligence = 1;

    Name = "Village hunter";
    Description = "Hunters in villages are respected, because in many ways it depends on them what the residents will have for dinner today. They are often taken into the army for their outstanding shooting skills";
    PrefabId = "u17";
    Type = UnitType.Range;
    MaxLevel = 7;
    LevelingCoreStat = CoreStat.Dexterity;
    TotalHealth = 20f;
    MoveSpeed = 3.5f;
    DefaultMovePoints = 5;
    Initiative = 7;
    Priority = 17;
    Projectiles = 25;
    BehaviorType = AIBehaviorType.KeepDistance;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Bow
    };
  }
}

