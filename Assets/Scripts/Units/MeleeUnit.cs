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

    float hitChance = Calculate.HitChance(this, Target, CurrentAttackType == AttackType.Charged);
    successAttack = Randomiser.RollChance(hitChance);

    if (!successAttack) {
      Target.Animator.Dodge();
      FailedAttacks++;
    } else {
      FailedAttacks = 0;
    }

    switch (CurrentAttackType) {
      case AttackType.Charged: Animator.ChargedAttack(); break;
      case AttackType.Fan: Animator.FanAttack(); break;
      default: Animator.Attack(); break;
    }

    Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
  }
}
