public class Silas : MeleeUnit {
  private Silas() {
    Strength = 2;
    Dexterity = 5;
    Intelligence = 1;

    Name = "Silas";
    Description = "One of the most dangerous criminals in these parts. He knows no mercy, and some even believe he is not human. A huge number of people followed him, mostly out of fear.";
    PrefabId = "u16";
    Type = UnitType.Melee;
    Size = ArmorSize.M;
    Level = 4;
    MaxLevel = 11;
    LevelingCoreStat = CoreStat.Dexterity;
    TotalHealth = 40f;
    MoveSpeed = 3f;
    DefaultMovePoints = 7;
    Initiative = 9;
    Priority = 8;
    BehaviorType = AIBehaviorType.PriorityTarget;
    IsBoss = true;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Dagger
    };
  }
}
