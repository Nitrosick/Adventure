using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnitCombat : Unit {
  private CancellationTokenSource phaseCts;
  private readonly int delayAfterAttack = 1000;

  public override async void BreakObject(Breakable target) {
    BattleUI.Instance.DisableUI();
    TargetObject = target;
    Vector3 dirToTarget = (TargetObject.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
    Animator.Attack();
  }

  public override async void ChopTree(TreeObject target) {
    BattleUI.Instance.DisableUI();
    TargetTree = target;
    Vector3 dirToTarget = (TargetTree.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
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

  public override void DealPierceDamage(bool charged = false) {
    if (Target == null) {
      NextPhase(true);
      return;
    }

    if (successAttack) {
      int obstacleLayer = LayerMask.NameToLayer("Obstacle");
      int unitLayer = LayerMask.NameToLayer("Unit");
      RaycastHit[] hits = Calculate.HitsOnTrajectory(CurrentTile, Target.CurrentTile);

      foreach (var hit in hits) {
        GameObject go = hit.collider.gameObject;

        if (go.layer == obstacleLayer) {
          Target.Ui.ShowPopup("Obstacle!");
          Target = null;
          NextPhase();
          return;
        }

        if (go.layer == unitLayer && !DamageBlocked(charged) && go.TryGetComponent<Unit>(out var unit)) {
          float critModifier = Calculate.CritModifier(this, unit, charged);
          float damage = Calculate.Damage(this, unit, charged);

          List<Effect> effects = Calculate.ItemEffects(this, unit);
          foreach (Effect effect in effects) unit.Effects.ApplyEffect(effect);

          unit.Health.TakeDamage(damage, critModifier);
          LogDamage(damage, critModifier, effects);
        }
      }
    } else {
      Target.Ui.ShowPopup("Miss!");
    }
  }

  protected override bool DamageBlocked(bool charged) {
    List<Skill> skills = Calculate.ItemPassiveSkills(Target);

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case "Parry":
          Target.Animator.Parry();
          Target.Ui.ShowPopup("Parry!");
          _ = CameraController.Shake(0.8f);
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
