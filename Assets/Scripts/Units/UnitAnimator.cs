using System.Threading.Tasks;
using UnityEngine;

public class UnitAnimator : MonoBehaviour {
  private Transform model;
  private Animator animator;

  private void Awake() {
    model = transform.Find("Model").transform;
    animator = model.GetComponent<Animator>();
    animator.SetFloat("Speed", transform.GetComponent<Unit>().MoveSpeed / 3.5f);

    if (model == null || animator == null) {
      Debug.LogError("Unit animator components initialization error");
    }
  }

  public void SetMoving(bool active) {
    animator.SetBool("IsMoving", active);
  }

  public void SetCrouching(bool active) {
    animator.SetBool("IsCrouching", active);
  }

  public void SetBlocking(bool active) {
    animator.SetBool("IsBlocking", active);
  }

  public void SetAttackType(int number) {
    animator.SetInteger("AttackType", number);
  }

  public void Attack() {
    animator.SetTrigger("Attack");
  }

  public void TakeDamage() {
    animator.SetTrigger("Damage");
  }

  public void Die() {
    animator.SetTrigger("Die");
  }

  public void Dodge() {
    animator.SetTrigger("Dodge");
  }

  public void Reset() {
    SetMoving(false);
    SetCrouching(false);
    SetBlocking(false);
  }

  public async Task RotateTowards(Vector3 direction, bool immediate = false, float intensity = 720f) {
    if (direction == Vector3.zero) return;
    direction.y = 0f;
    if (direction == Vector3.zero) return;
    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

    if (immediate) {
      model.rotation = targetRotation;
      return;
    }

    while (Quaternion.Angle(model.rotation, targetRotation) > 0.5f) {
      model.rotation = Quaternion.RotateTowards(
        model.rotation,
        targetRotation,
        intensity * Time.deltaTime
      );
      await Task.Yield();
    }

    model.rotation = targetRotation;
  }
}
