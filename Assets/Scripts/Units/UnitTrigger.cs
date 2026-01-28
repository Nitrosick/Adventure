using UnityEngine;

public class UnitTrigger : MonoBehaviour {
  Unit unit;

  private void Awake() {
    unit = GetComponentInParent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit trigger components initialization error");
    }
  }

  private void TriggerAttack() {
    unit.DealDamage();
  }

  private void TriggerChargedAttack() {
    unit.DealDamage(charged: true);
  }

  private void TriggerPierceAttack() {
    unit.DealPierceDamage();
  }

  private void TriggerCrossbowShoot() {
    unit.CrossbowShoot();
  }

  private void TriggerBowShoot() {
    unit.BowShoot();
  }
}
