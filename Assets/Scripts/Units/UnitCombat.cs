using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnitCombat : Unit {
  private CancellationTokenSource phaseCts;
  private readonly int delayAfterAttack = 1000;

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
    }
    else {
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
        if (!DamageBlocked(charged)) {
          BlockBreakdown(charged);

          float critModifier = Calculate.CritModifier(this, Target, charged);
          float damage = Calculate.Damage(this, Target, charged);

          List<Effect> effects = Calculate.ItemEffects(this, Target);
          foreach (Effect effect in effects) Target.Effects.ApplyEffect(effect);

          Target.Health.TakeDamage(damage, critModifier, charged: charged);
          LogDamage(damage, critModifier, effects);
        }
      } else {
        Target.Ui.ShowPopup("Miss!");
      }
    }

    if (TargetObject != null) TargetObject.Break();
    if (TargetTree != null) TargetTree.Chop();
    NextPhase();
  }

  protected override bool DamageBlocked(bool charged) {
    List<Skill> skills = Calculate.ItemPassiveSkills(Target);

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case "Parry":
          Target.Animator.Parry();
          Target.Ui.ShowPopup("Parry!");
          Target.Health.TakeDamage(0f, 1f);
          return true;
      }
    }

    return false;
  }

  protected void BlockBreakdown(bool charged) {
    if (!Target.Effects.HasAnyEffect(new string[] { "Wall", "Block" })) return;

    if (
      Equip.primary.damageType == DamageType.Chop ||
      Equip.primary.damageType == DamageType.Crash
    ) {
      float chance = Equip.primary.armorPenetration;
      if (charged) chance *= 2;
      bool isBreak = Utils.RollChance(chance);

      if (isBreak) {
        Target.Effects.ClearEffect("Wall");
        Target.Effects.ClearEffect("Block");
      }
    }
  }

  public override void Shoot() {
    CurrentProjectiles -= 1;
    if (CurrentProjectiles == 0) BehaviorType = AIBehaviorType.Retreat;
  }

  public override void BlockStance(string id) {
    Effect effect = Factory.CreateEffectById(id);
    if (effect == null) return;

    Effects.ApplyEffect(effect);
    Animator.SetBlocking(true);
    SkillCharges -= 1;

    if (SkillCharges <= 0) BattleUI.Instance.DisableSkills();
    if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    NextPhase(true);
  }

  protected override void LogDamage(float damage, float critModifier, List<Effect> effects) {
    string damageVal = ((float)Math.Round(damage * critModifier, 1)).ToString();
    if (critModifier > 1) damageVal = $"<color=#EFBF0D>{damageVal}</color>";
    LogUI.Instance.Add($"{Name} <color=#A0A0A0>deals</color> {damageVal} <color=#A0A0A0>damage to</color> {Target.Name}");

    foreach (Effect e in effects) {
      string effectText = $"<color={(e.isNegative ? "#F61010" : "#81D11F")}>{e.effectName}</color>";
      LogUI.Instance.Add($"{Target.Name} <color=#A0A0A0>is affected by</color> {effectText}");
    }
  }

  public override async void NextPhase(bool instant = false) {
    phaseCts?.Cancel();
    phaseCts = new CancellationTokenSource();
    CancellationToken token = phaseCts.Token;

    try {
      if (!instant) await Task.Delay(delayAfterAttack, token);
      Target = null;
      TargetObject = null;
      TargetTree = null;

      PhaseManager.NextPhase();
    } catch (TaskCanceledException) { }
  }
}
