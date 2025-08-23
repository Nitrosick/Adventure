using System.Threading.Tasks;
using UnityEngine;

public class UnitHealth : MonoBehaviour {
  private Unit unit;
  protected readonly int objectDestroyTime = 10;

  private void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit health components initialization error");
    }
  }

  public void TakeDamage(float damage, float modifier, bool isTickDamage = false) {
    float totalDamage = damage * modifier;

    if (modifier > 1f) {
      unit.Ui.ShowPopup(totalDamage.ToString(), PopupType.Crit);
      if (!isTickDamage) CameraController.Shake(1.2f);
    }
    else {
      unit.Ui.ShowPopup(totalDamage.ToString(), PopupType.Negative);
      if (!isTickDamage) CameraController.Shake(0.8f);
    }

    if (totalDamage >= unit.CurrentHealth) {
      unit.CurrentHealth = 0;
      Die();
    }
    else {
      unit.CurrentHealth -= totalDamage;
      unit.Ui.UpdateHealth(unit.TotalHealth, unit.CurrentHealth);
      if (!isTickDamage) unit.Animator.TakeDamage();
    }

    if (unit.BehaviorType == AIBehaviorType.HoldPosition) {
      unit.BehaviorType = AIBehaviorType.KeepDistance;
    }
  }

  private void Die() {
    unit.IsDead = true;
    unit.CurrentTile.OccupiedBy = null;
    unit.UnitCollider.enabled = false;
    unit.Ui.ClearMarkers();
    unit.Ui.HideHealthBar();
    unit.Ui.HideChargesBar();
    unit.Effects.ClearEffects();
    unit.Animator.Die();
    _ = MakeCorpse();
  }

  public void Heal(float value, bool inBattle = true) {
    unit.CurrentHealth += value;
    if (unit.CurrentHealth > unit.TotalHealth) unit.CurrentHealth = unit.TotalHealth;
    if (!inBattle) Player.Instance.Army.UpdateState();
  }

  private async Task MakeCorpse() {
    await Task.Delay(objectDestroyTime * 1000);

    GameObject model = transform.Find("Model").gameObject;
    Destroy(model);

    Instantiate(
      BattleManager.Instance.corpsePrefab,
      transform.position,
      Quaternion.Euler(0, 65, 0)
    );
  }
}
