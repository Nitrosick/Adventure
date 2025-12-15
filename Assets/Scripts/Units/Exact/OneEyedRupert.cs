public class OneEyedRupert : UnitCombat {
  private OneEyedRupert() {
    Strength = 6;
    Dexterity = 1;
    Intelligence = 1;

    Name = "One-eyed Rupert";
    Description = "This brute keeps the whole neighborhood in fear. He gets too bored after months of sailing and starts brawls in taverns and on the street. It won't be easy to take down such a giant, because in addition to his size, he's quite skilled with a saber.";
    PrefabId = "u9";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    Size = ArmorSize.L;
    Level = 3;
    MaxLevel = 10;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 45f;
    MoveSpeed = 2.8f;
    DefaultMovePoints = 4;
    Initiative = 6;
    Priority = 5;
    BehaviorType = AIBehaviorType.PriorityTarget;
    IsBoss = true;
  }
}
