using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
  private Animator animator;

  private void Awake() {
    animator = transform.Find("Model").GetComponent<Animator>();
    if (animator == null) Debug.LogError("Player animation initialization error");
  }

  public void SetMoving(bool isMoving) {
    animator.SetBool("IsMoving", isMoving);
  }

  public void RotateTowards(Vector3 direction) {
    if (direction == Vector3.zero) return;

    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

    transform.rotation = Quaternion.RotateTowards(
      transform.rotation,
      targetRotation,
      720f * Time.deltaTime
    );
  }
}
