using UnityEngine;

public class PlayerAnimator : MonoBehaviour {
  private Animator animator;

  void Awake() {
    animator = transform.Find("Model").GetComponent<Animator>();
    if (animator == null) Debug.LogError("Player animation initialization error");
  }

  public void SetMoving(bool isMoving) {
    animator.SetBool("IsMoving", isMoving);
  }

  public void SetTorch(bool withTorch) {
    animator.SetBool("Torch", withTorch);
  }

  public void RotateTowards(Vector3 direction) {
    direction.y = 0f;
    if (direction == Vector3.zero) return;

    float targetY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    Quaternion targetRotation = Quaternion.Euler(0f, targetY, 0f);

    transform.rotation = Quaternion.RotateTowards(
      transform.rotation,
      targetRotation,
      720f * Time.deltaTime
    );

    Vector3 euler = transform.eulerAngles;
    transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
  }
}
