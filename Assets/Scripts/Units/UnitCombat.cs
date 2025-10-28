using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnitCombat : Unit {
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
    Animator.SetAttackType(Equip.primary.attackType);

    if (IsChargedAttack) {
      Animator.ChargedAttack();
      SkillCharges -= 1;
      if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    } else {
      Animator.Attack();
    }
    IsChargedAttack = false;
  }

  public override async void BreakObject(Breakable target) {
    BattleUI.Instance.DisableUI();
    TargetObject = target;
    Vector3 dirToTarget = (TargetObject.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
    Animator.SetAttackType(Equip.primary.attackType);
    Animator.Attack();
  }

  public override async void ChopTree(TreeObject target) {
    BattleUI.Instance.DisableUI();
    TargetTree = target;
    Vector3 dirToTarget = (TargetTree.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
    Animator.SetAttackType(Equip.primary.attackType);
    Animator.Attack();
  }

  public override void DealDamage(bool charged = false) {
    if (Target != null) {
      if (successAttack) {
        if (!DamageBlocked()) {
          float critModifier = Calculate.CritModifier(this, Target, charged);
          float damage = Calculate.Damage(this, Target, charged);
          List<Effect> effects = Calculate.ItemEffects(this, Target);
          foreach (Effect effect in effects) Target.Effects.ApplyEffect(effect);
          Target.Health.TakeDamage(damage, critModifier);
        }
      } else {
        Target.Ui.ShowPopup("Miss!");
      }
      Target = null;
    }

    if (TargetObject != null) {
      TargetObject.Break();
      _ = CameraController.Shake(0.8f);
      TargetObject = null;
      FinishAction();
    }

    if (TargetTree != null) {
      TargetTree.Chop();
      _ = CameraController.Shake(0.8f);
      TargetTree = null;
      FinishAction();
    }
  }

  protected override bool DamageBlocked() {
    List<Skill> skills = Calculate.ItemPassiveSkills(Target);

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case SkillName.Parry:
          Target.Animator.Parry();
          Target.Ui.ShowPopup("Parry!");
          Target.Health.TakeDamage(0f, 1f);
          return true;
      }
    }

    if (Target.Effects.HasEffect("Wall")) {
      if (
        Equip.primary.damageType == DamageType.Chop ||
        Equip.primary.damageType == DamageType.Crash
      ) {
        bool isBreak = Utils.RollChance(Equip.primary.armorPenetration);
        if (isBreak) {
          Target.Effects.ClearEffect("Wall");
          return false;
        }
      }

      Target.Ui.ShowPopup("Block!");
      Target.Health.TakeDamage(0f, 1f);
      return true;
    }

    return false;
  }

  public override void FinishAction() {
    if (!PreventPhaseSkip) PhaseManager.NextPhase();
    PreventPhaseSkip = false;
  }

  public override void Shoot() {
    CurrentProjectiles -= 1;
    if (CurrentProjectiles == 0) BehaviorType = AIBehaviorType.Retreat;
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
