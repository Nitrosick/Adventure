public class Militia : Unit
{
  private Militia() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Militia";
    Description = "Yesterday's peasant is today's armed infantryman. An inexperienced and poorly equipped fighter, but always ready for battle.";
    prefabId = 2;
    Type = UnitType.Melee;
    TotalHealth = 25f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 10;
  }
}
