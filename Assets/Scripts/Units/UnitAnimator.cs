using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

public class UnitAnimator : MonoBehaviour {
  private Unit unit;
  private Transform model;
  private Animator animator;

  void Awake() {
    unit = transform.GetComponent<Unit>();
    model = transform.Find("Model").transform;
    animator = model.GetComponent<Animator>();
    animator.SetFloat("Speed", transform.GetComponent<Unit>().MoveSpeed / 3.5f);

    if (unit == null || model == null || animator == null) {
      Debug.LogError("Unit animator components initialization error");
    }
  }

  void Start() {
    FocusToPoint();
  }

  public void SetController(AnimatorController controller) {
    animator.runtimeAnimatorController = controller;
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

  public void SetStunned(bool active) {
    animator.SetBool("IsStunned", active);
  }

  public void SetRooted(bool active) {
    animator.SetBool("IsRooted", active);
  }

  public void Attack() {
    animator.SetTrigger("Attack");
  }

  public void ChargedAttack() {
    animator.SetTrigger("ChargedAttack");
  }

  public void FanAttack() {
    animator.SetTrigger("FanAttack");
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

  public void Parry() {
    animator.SetTrigger("Parry");
  }

  public void Reset() {
    SetMoving(false);
    SetCrouching(false);
    SetBlocking(false);
    SetStunned(false);
    SetRooted(false);
  }

  private void FocusToPoint() {
    if (unit.CurrentTile == null) return;

    Tile focusTile = unit.Relation == UnitRelation.Ally
      ? TileManager.allyFocusTile
      : TileManager.enemyFocusTile;

    if (focusTile == null) return;

    Vector3 center = unit.CurrentTile.GetPos();
    Vector3 focusPos = focusTile.GetPos();
    Vector3 from = new (center.x, 0, center.z);
    Vector3 to = new (focusPos.x, 0, focusPos.z);
    Vector3 direction = (to - from).normalized;

    _ = RotateTowards(direction, true);
  }

  public async Task RotateTowards(Vector3 direction, bool immediate = false, float intensity = 720f) {
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
