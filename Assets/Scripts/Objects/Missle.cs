using UnityEngine;

public class Missle : MonoBehaviour {
  private Rigidbody rb;
  private Unit source;
  private float damage;
  private float critModifier;
  private bool success;
  private readonly int defaultDestroyTime = 5;

  private void Awake() {
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
    Destroy(gameObject, defaultDestroyTime);
  }

  private void OnTriggerEnter(Collider other) {
    if (other == source.GetComponent<Collider>()) return;

    if (other.gameObject.CompareTag("Unit")) {
      Unit target = other.GetComponent<Unit>();

      if (target.Effects.HasEffect("Block")) {
        target.Ui.ShowPopup("Block!");
        target.Health.TakeDamage(0f, 1f);
      } else if (success) {
        target.Health.TakeDamage(damage, critModifier);
      } else {
        target.Ui.ShowPopup("Miss!");
        target.Animator.Dodge();
      }
    } else {
      PhaseManager.NextPhase();
    }

    Destroy(gameObject);
  }
}
