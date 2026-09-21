using System;
using UnityEngine;

public class Trap : MonoBehaviour {
  public Effect effect;
  public int effectDuration = 1;
  public float damage;

  public bool hidden;
  public UnitRelation Relation { get; private set; }
  public bool IsHidden { get; private set; } = true;
  public TrapType Type { get; private set; }

  public void Init(UnitRelation relation, TrapType type) {
    Relation = relation;
    hidden = relation == UnitRelation.Enemy;
    Type = type;
  }

  private string G(string text) => Utils.GreyText(text);

  public Trap Reveal() {
    if (!hidden) return this;

    GameObject prefab = BattleManager.Instance
      .trapRegistry
      .Get(Type);

    if (prefab == null) return null;

    GameObject obj = Instantiate(
      prefab,
      transform.parent
    );

    obj.transform.position = transform.position;
    Trap visibleTrap = obj.GetComponent<Trap>();
    Destroy(gameObject);
    return visibleTrap;
  }

  public void Trigger(Unit unit) {
    if (hidden) {
      Trap visibleTrap = Reveal();
      if (visibleTrap != null) visibleTrap.Trigger(unit);
      return;
    }

    transform.GetComponent<Animator>().SetTrigger("Trigger");

    if (effect == null || unit == null) return;
    float avoidChance = 0f;

    if (unit.IsHero) avoidChance += AbilityController.AmbushProtectBonus();

    if (Randomiser.RollChance(avoidChance)) {
      unit.Ui.ShowPopup("Avoid!");
      return;
    }

    float totalDamage = damage;

    if (unit.Relation == UnitRelation.Ally)
      totalDamage *= AbilityController.TrapsResistBonus();

    Armor armor = unit.Equip.armor;

    float modifier = armor != null && armor.weight == EquipmentWeight.Heavy ? 0.5f : 1f;

    if (totalDamage > 0)
      unit.Health.TakeDamage(
        totalDamage,
        modifier,
        true
      );

    _ = CameraController.Shake(0.8f);

    unit.Effects.ApplyEffect(
      effect,
      effectDuration
    );

    LogDamage(
      unit,
      totalDamage,
      modifier,
      effect
    );

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
