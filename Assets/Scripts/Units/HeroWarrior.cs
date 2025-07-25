using UnityEngine;

public class HeroWarrior : Unit {
  private HeroWarrior() {
    Strength = 5;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Hero";
    Description = "The fate of this warrior is entirely in your hands.";
    IsHero = true;
    prefabId = "u1";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    ShieldIsAllow = true;
    TotalHealth = 40f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 7;
    Priority = 6;
  }

  public override void Block() {
    Effect effect = Resources.Load<Effect>("Effects/Block");
    if (effect == null) return;
    Effects.ApplyEffect(effect);
    Animator.SetBlocking(true);
    SkillCharges -= 1;
    if (SkillCharges <= 0) BattleUI.DisableSkills();
    if (Equip.GetSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    FinishAction();
  }
}
