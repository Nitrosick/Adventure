using System;
using UnityEngine;

public class Trap : MonoBehaviour {
  public Effect effect;
  public int effectDuration = 1;
  public float damage;

  public UnitRelation Relation { get; private set; }
  public bool IsHidden { get; private set; } = true;
  public TrapType Type { get; private set; }

  public void Init(UnitRelation relation, TrapType type) {
    Relation = relation;
    Type = type;
  }

  private string G(string text) => Utils.GreyText(text);

  public void Trigger(Unit unit) {
    transform.GetComponent<Animator>().SetTrigger("Trigger");
    if (effect == null || unit == null) return;

    float avoidChance = 0f;
    if (unit.IsHero) avoidChance += AbilityController.AmbushProtectBonus();
    if (Randomiser.RollChance(avoidChance)) {
      unit.Ui.ShowPopup("Avoid!");
      return;
    }

    float totalDamage = damage;
    if (unit.Relation == UnitRelation.Ally) totalDamage *= AbilityController.TrapsResistBonus();
    Armor armor = unit.Equip.armor;
    float modifier = armor != null && armor.weight == EquipmentWeight.Heavy ? 0.5f : 1f;
    if (totalDamage > 0) unit.Health.TakeDamage(totalDamage, modifier, true);
    _ = CameraController.Shake(0.8f);

    unit.Effects.ApplyEffect(effect, effectDuration);
    LogDamage(unit, totalDamage, modifier, effect);

    if (unit.IsDead) return;

    if (effect.cancelMove) {
      unit.CurrentMovePoints = 0;
      PhaseManager.NextPhase();
    }
  }

  private void LogDamage(
    Unit unit,
    float damage,
    float modifier,
    Effect effect
  ) {
    string damageVal = ((float)Math.Round(damage * modifier, 1)).ToString();
    LogUI.Instance.Add($"{unit.Name} {G("takes")} {damageVal} {G("damage from the")} Trap");

    if (effect != null) {
      string effectText = $"<color={(effect.isNegative ? "#F61010" : "#81D11F")}>{effect.effectName}</color>";
      LogUI.Instance.Add($"{unit.Name} {G("is affected by")} {effectText}");
    }
  }
}
