using System.Threading.Tasks;
using UnityEngine;

public class MeleeUnit : UnitCombat {
  public override async void OnAttack(Unit target = null) {
    BattleUI.Instance.DisableUI();
    if (target != null) Target = target;

    Vector3 dirToTarget = (Target.transform.position - transform.position).normalized;
    Vector3 dirFromTarget = (transform.position - Target.transform.position).normalized;

    await Task.WhenAll(
      Animator.RotateTowards(dirToTarget),
      Target.Animator.RotateTowards(dirFromTarget)
    );

    float hitChance = Calculate.HitChance(this, Target, IsChargedAttack);
    successAttack = Utils.RollChance(hitChance);

    if (!successAttack) Target.Animator.Dodge();

    if (IsChargedAttack) {
      Animator.ChargedAttack();
      if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    } else {
      Animator.Attack();
    }
    IsChargedAttack = false;
  }

  public override void BlockStance(string id) {
    Effect effect = Factory.CreateEffectById(id);
    if (effect == null) return;

    Effects.ApplyEffect(effect);
    Animator.SetBlocking(true);

    if (SkillCharges <= 0) BattleUI.Instance.DisableSkills();
    if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    NextPhase(true);
  }
}
