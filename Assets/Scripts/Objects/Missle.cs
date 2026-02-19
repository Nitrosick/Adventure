using System;
using System.Threading.Tasks;
using UnityEngine;

public class Missle : MonoBehaviour {
  private Rigidbody rb;
  private Unit source;
  private float damage;
  private float critModifier;
  private bool success;
  private readonly int delayAfterCollision = 1000;
  private bool triggerHandled;

  void Awake() {
    rb = transform.GetComponent<Rigidbody>();
    if (rb == null) Debug.LogError("Missle initialization error");
  }

  void LateUpdate() {
    Vector3 v = rb.velocity;
    if (v.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(v);
  }

  public void Launch(Unit src, Vector3 velocity, float dmg, float crit, bool suc) {
    source = src;
    damage = dmg;
    critModifier = crit;
    success = suc;
    rb.isKinematic = false;
    rb.velocity = velocity;
  }

  private async void OnTriggerEnter(Collider other) {
    if (other == source.GetComponent<Collider>()) return;
    if (triggerHandled) return;
    triggerHandled = true;

    if (other.gameObject.CompareTag("Unit")) {
      Unit target = other.GetComponent<Unit>();

      if (target.Effects.HasAnyEffect(new string[] { "Block", "Wall" })) {
        // FIXME: Почему-то не срабатывает
        _ = CameraController.Shake(0.8f);
        target.Ui.ShowPopup("Block!");
      } else if (success) {
        target.Health.TakeDamage(damage, critModifier);
        LogDamage(target, damage, critModifier);
      } else {
        target.Ui.ShowPopup("Miss!");
        target.Animator.Dodge();
      }
    }

    Destroy(gameObject);
    await Task.Delay(delayAfterCollision);
    PhaseManager.NextPhase();
  }

  private void LogDamage(Unit unit, float damage, float critModifier) {
    string damageVal = ((float)Math.Round(damage * critModifier, 1)).ToString();
    if (critModifier > 1) damageVal = $"<color=#EFBF0D>{damageVal}</color>";
    LogUI.Instance.Add($"<color=#A0A0A0>The projectile deals</color> {damageVal} <color=#A0A0A0>damage to</color> {unit.Name}");
  }
}
