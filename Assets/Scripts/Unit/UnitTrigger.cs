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

  private void TriggerShoot() {
    unit.Shoot();
  }

  private void FinishAction() {
    unit.FinishAction();
  }
}
