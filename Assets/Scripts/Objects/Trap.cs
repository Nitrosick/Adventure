using UnityEngine;

public class Trap : MonoBehaviour {
  public Effect effect;
  public float damage;

  public UnitRelation Relation { get; private set; }
  public bool IsHidden { get; private set; } = true;
  public TrapType Type { get; private set; }

  public void Init(UnitRelation relation, TrapType type) {
    Relation = relation;
    Type = type;
  }

  public void Trigger(Unit unit) {
    transform.GetComponent<Animator>().SetTrigger("Trigger");
    if (effect == null || unit == null) return;

    float modifier = unit.Equip.armor.weight == EquipmentWeight.Heavy ? 0.5f : 1f;
    if (damage > 0) unit.Health.TakeDamage(damage, modifier, true);

    if (unit.IsDead) {
      QueueManager.NextUnit();
      return;
    }

    unit.Effects.ApplyEffect(effect);
    if (effect.cancelMove) PhaseManager.NextPhase();
  }
}
