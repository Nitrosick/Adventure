using UnityEngine;

public class Missle : MonoBehaviour
{
  private Rigidbody rb;
  private Unit source;
  private Vector3 direction;
  private int speed;
  private float damage;
  private float critModifier;
  private bool success;

  private void Awake() {
    rb = transform.GetComponent<Rigidbody>();
    if (rb == null) Debug.LogError("Missle initialization error");
  }

  public void Launch(Unit src, Vector3 dir, int spd, float dmg, float crit, bool suc) {
    source = src;
    direction = dir;
    speed = spd;
    damage = dmg;
    critModifier = crit;
    success = suc;
    rb.isKinematic = false;
    rb.velocity = direction * speed;
  }

  private void OnTriggerEnter(Collider other) {
    if (other == source.GetComponent<Collider>()) return;

    if (other.gameObject.CompareTag("Unit")) {
      Unit target = other.GetComponent<Unit>();

      if (target.Effects.HasEffect("Block")) {
        target.Ui.ShowPopup("Block!");
        target.TakeDamage(0f, 1f);
      } else if (success) {
        target.TakeDamage(damage, critModifier);
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
