using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEffects : MonoBehaviour {
  private Unit unit;
  public List<Skill> innateSkills = new();
  public List<EffectInstance> ActiveEffects { get; private set; } = new();

  void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit effect components initialization error");
    }
  }

  public void ApplyEffect(Effect effect, int duration = 0, float damage = 0) {
    if (unit.IsDead) return;
    EffectInstance existing = ActiveEffects.Find(e => e.effectData == effect);

    if (existing != null) {
      if (effect.isStackable) existing.remainingTurns += effect.duration;
      else existing.remainingTurns = effect.duration;
    }
    else {
      ActiveEffects.Add(new EffectInstance(effect, duration, damage));
    }

    if (effect.effectName == "Stun") unit.Animator.SetStunned(true);
    if (effect.effectName == "Root") unit.Animator.SetRooted(true);

    unit.Ui.UpdateEffects();
  }

  public void ProcessTurnEffects() {
    if (unit == null || ActiveEffects == null || ActiveEffects.Count == 0) return;

    for (int i = ActiveEffects.Count - 1; i >= 0; i--) {
      if (i >= ActiveEffects.Count) continue;

      EffectInstance instance = ActiveEffects[i];

      if (instance == null) {
        ActiveEffects.RemoveAt(i);
        continue;
      }

      instance.Tick(unit);

      if (instance == null || instance.IsExpired) {
        if (i < ActiveEffects.Count) ActiveEffects.RemoveAt(i);
      }
    }

    if (unit.Ui != null) unit.Ui.UpdateEffects();
  }


  public bool PreventsTurn() {
    return ActiveEffects.Any(e => e.effectData.cancelAttack || e.effectData.cancelMove);
  }

  public bool HasEffect(string effectName) {
    return ActiveEffects.Any(e => e.effectData != null && e.effectData.effectName == effectName);
  }

  public bool HasAnyEffect(string[] effectNames) {
    return ActiveEffects.Any(e => e.effectData != null && effectNames.Contains(e.effectData.effectName));
  }

  public void ClearEffect(string effectName) {
    ActiveEffects.RemoveAll(e => e.effectData != null && e.effectData.effectName == effectName);
    if (effectName == "Stun") unit.Animator.SetStunned(false);
    if (effectName == "Root") unit.Animator.SetRooted(false);
    else if (effectName == "Wall" || effectName == "Block") unit.Animator.SetBlocking(false);
    unit.Ui.UpdateEffects();
  }

  public void ClearEffects() {
    ActiveEffects.Clear();
    unit.Animator.SetStunned(false);
    unit.Animator.SetRooted(false);
    unit.Ui.UpdateEffects();
  }
}
