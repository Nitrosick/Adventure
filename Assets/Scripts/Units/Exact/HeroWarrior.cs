using UnityEngine;

public class HeroWarrior : UnitCombat {
  private HeroWarrior() {
    Strength = 5;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Hero";
    Description = "The fate of this warrior is entirely in your hands.";
    IsHero = true;
    PrefabId = "u1";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.OneHandWeapon;
    MaxLevel = 30;
    LevelingCoreStat = CoreStat.Strength;
    ShieldIsAllow = true;
    TotalHealth = 40f;
    MoveSpeed = 3f;
    DefaultMovePoints = 6;
    Initiative = 7;
    Priority = 6;
    BehaviorType = AIBehaviorType.Passive;
  }

  public override void BlockStance(SkillName type) {
    Effect effect = Resources.Load<Effect>("Effects/" + type.ToString());
    if (effect == null) return;
    Effects.ApplyEffect(effect);
    Animator.SetBlocking(true);
    SkillCharges -= 1;
    if (SkillCharges <= 0) BattleUI.Instance.DisableSkills();
    if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    FinishAction();
  }
}
