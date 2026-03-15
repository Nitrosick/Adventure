public class Deserter : MeleeUnit {
  private Deserter() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Deserter";
    Description = "Not everyone can handle the burden of the town guard. Some resign from service, while others desert and join bandits.";
    PrefabId = "u15";
    Type = UnitType.Melee;
    Level = 1;
    MaxLevel = 5;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 10;
    BehaviorType = AIBehaviorType.TryPierceHit;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Spear
    };
  }
}
