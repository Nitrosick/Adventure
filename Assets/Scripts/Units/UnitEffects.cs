using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEffects : MonoBehaviour {
  private Unit unit;
  public List<EffectInstance> ActiveEffects { get; private set; } = new();

  private void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit effect components initialization error");
    }
  }

  public void ApplyEffect(Effect effect, int duration = 0, float damage = 0) {
    var existing = ActiveEffects.Find(e => e.effectData == effect);

    if (existing != null && !effect.isStackable) {
      existing.remainingTurns = effect.duration;
    }
    else {
      if (duration == 0 && damage == 0) ActiveEffects.Add(new EffectInstance(effect));
      else ActiveEffects.Add(new EffectInstance(effect, duration, damage));
    }
    unit.Ui.UpdateEffects();
  }

  public void ProcessTurnEffects() {
    for (int i = ActiveEffects.Count - 1; i >= 0; i--) {
      EffectInstance instance = ActiveEffects[i];
      instance.Tick(unit);
      if (instance.IsExpired) ActiveEffects.RemoveAt(i);
      unit.Ui.UpdateEffects();
    }
  }

  public bool PreventsTurn() {
    return ActiveEffects.Any(e => e.effectData.cancelAttack || e.effectData.cancelMove);
  }

  public bool HasEffect(string effectName) {
    return ActiveEffects.Any(e => e.effectData != null && e.effectData.name == effectName);
  }

  public void ClearEffect(string effectName) {
    ActiveEffects.RemoveAll(e => e.effectData != null && e.effectData.name == effectName);
    unit.Ui.UpdateEffects();
  }

  public void ClearEffects() {
    ActiveEffects.Clear();
    unit.Ui.UpdateEffects();
  }
}
