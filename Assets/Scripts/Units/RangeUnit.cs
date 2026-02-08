using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RangeUnit : UnitCombat {
  protected struct RangedAttackData {
    public float damage;
    public float critModifier;
    public bool success;
  }

  private Transform weapon;
  private Animator weaponAnimation;
  private Transform missleSpawner;
  private GameObject misslePrefab;
  private float missleSpeed = 1f;

  public async override void Init(Tile tile, UnitRelation relation) {
    base.Init(tile, relation);

    await Task.Yield();
    weapon = GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.CompareTag("Weapon"));
    weaponAnimation = weapon.GetComponent<Animator>();
    missleSpawner = weapon.transform.Find("MissleSpawner").GetComponent<Transform>();
    misslePrefab = Equip.primary.misslePrefab;
    missleSpeed = Equip.primary.missleSpeed;

    if (weapon == null || missleSpawner == null || misslePrefab == null) {
      Debug.LogError("Crossbowman components initialization error");
    }
  }

  public async override void OnAttack(Unit target = null) {
    BattleUI.Instance.DisableUI();
    _ = CameraController.FocusOn(transform.position);
    if (target != null) Target = target;

    Vector3 dirToTarget = (Target.transform.position - transform.position).normalized;
    Vector3 dirFromTarget = (transform.position - Target.transform.position).normalized;

    await Task.WhenAll(
      Animator.RotateTowards(dirToTarget),
      Target.Animator.RotateTowards(dirFromTarget)
    );

    Animator.Attack();
    if (weaponAnimation != null) weaponAnimation.SetTrigger("Shoot");
  }

  protected RangedAttackData PrepareShoot() {
    base.Shoot();

    _ = CameraController.FocusOn(Target.transform.position);

    float hitChance = Calculate.HitChance(this, Target);
    float critModifier = Calculate.CritModifier(this, Target);
    float damage = Calculate.Damage(this, Target);
    bool success = Utils.RollChance(hitChance);

    if (success) FailedAttacks = 0;
    else FailedAttacks++;

    return new RangedAttackData {
      damage = damage,
      critModifier = critModifier,
      success = success
    };
  }

  protected Vector3 CalculateBowVelocity(
    Vector3 start,
    Vector3 end,
    float speed) {
    Vector3 toTarget = end - start;
    Vector3 toTargetXZ = new(toTarget.x, 0, toTarget.z);

    float distXZ = toTargetXZ.magnitude;
    float heightDiff = toTarget.y;
    float g = Mathf.Abs(Physics.gravity.y);

    float speed2 = speed * speed;
    float underSqrt = speed2 * speed2
        - g * (g * distXZ * distXZ + 2 * heightDiff * speed2);

    if (underSqrt <= 0f)
      return toTarget.normalized * speed;

    float sqrt = Mathf.Sqrt(underSqrt);
    float lowAngle = Mathf.Atan((speed2 - sqrt) / (g * distXZ));

    float vy = speed * Mathf.Sin(lowAngle);
    float vxz = speed * Mathf.Cos(lowAngle);

    Vector3 velocity = toTargetXZ.normalized * vxz;
    velocity.y = vy;

    return velocity;
  }


  protected void LaunchMissle(
    Missle missile,
    Vector3 velocity,
    RangedAttackData data) {
    missile.Launch(this, velocity, data.damage, data.critModifier, data.success);
  }

  public override void CrossbowShoot() {
    var data = PrepareShoot();
    Vector3 dir = (Target.UnitCollider.bounds.center - missleSpawner.position).normalized;

    var missile = Instantiate(
      misslePrefab,
      missleSpawner.position,
      Quaternion.LookRotation(dir)
    ).GetComponent<Missle>();

    LaunchMissle(missile, dir * missleSpeed, data);
  }

  public override void BowShoot() {
    var data = PrepareShoot();

    Missle arrow = Instantiate(
      misslePrefab,
      missleSpawner.position,
      Quaternion.identity
    ).GetComponent<Missle>();

    Vector3 velocity = CalculateBowVelocity(
      missleSpawner.position,
      Target.transform.position + Vector3.up,
      missleSpeed
    );

    LaunchMissle(arrow, velocity, data);
  }
}
