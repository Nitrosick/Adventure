using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Poacher : Unit {
  private Poacher() {
    Strength = 2;
    Dexterity = 4;
    Intelligence = 1;

    Name = "Poacher";
    Description = "They hunt animals, especially rare ones. But when money is really scarce, they won't hesitate to join a bandit group";
    PrefabId = "u6";
    Type = UnitType.Range;
    AllowedWeapon = EquipmentType.Bow;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Dexterity;
    TotalHealth = 20f;
    MoveSpeed = 3.5f;
    DefaultMovePoints = 5;
    Initiative = 8;
    Priority = 16;
    Projectiles = 25;
    BehaviorType = AIBehaviorType.KeepDistance;
  }

  public GameObject arrowPrefab;
  private Transform weapon;
  private Animator weaponAnimation;
  private Transform missleSpawner;
  private readonly float arrowSpeed = 15;

  public async override void Init(Tile tile, UnitRelation relation) {
    base.Init(tile, relation);

    await Task.Yield();
    weapon = GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.CompareTag("Weapon"));
    weaponAnimation = weapon.GetComponent<Animator>();
    missleSpawner = weapon.transform.Find("MissleSpawner").GetComponent<Transform>();

    if (weapon == null || missleSpawner == null) {
      Debug.LogError("Poacher components initialization error");
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

  public override void Shoot() {
    base.Shoot();

    _ = CameraController.FocusOn(Target.transform.position);
    Missle arrow = Instantiate(arrowPrefab, missleSpawner.position, Quaternion.identity)
      .GetComponent<Missle>();

    float hitChance = Calculate.HitChance(this, Target);
    float critModifier = Calculate.CritModifier(this, Target);
    float damage = Calculate.Damage(this, Target);
    bool successAttack = Utils.RollChance(hitChance);

    Vector3 start = missleSpawner.position;
    Vector3 end = Target.transform.position + Vector3.up;
    Vector3 toTarget = end - start;
    Vector3 toTargetXZ = new (toTarget.x, 0, toTarget.z);
    float distXZ = toTargetXZ.magnitude;
    float heightDiff = toTarget.y;
    float g = Mathf.Abs(Physics.gravity.y);

    float speed2 = arrowSpeed * arrowSpeed;
    float underSqrt = speed2 * speed2 - g * (g * distXZ * distXZ + 2 * heightDiff * speed2);

    if (underSqrt <= 0f) {
      Vector3 flatVel = toTarget.normalized * arrowSpeed;
      arrow.Launch(this, flatVel, damage, critModifier, successAttack);
      return;
    }

    float sqrt = Mathf.Sqrt(underSqrt);
    float lowAngle = Mathf.Atan((speed2 - sqrt) / (g * distXZ));
    float vy = arrowSpeed * Mathf.Sin(lowAngle);
    float vxz = arrowSpeed * Mathf.Cos(lowAngle);

    Vector3 velocity = toTargetXZ.normalized * vxz;
    velocity.y = vy;

    arrow.Launch(this, velocity, damage, critModifier, successAttack);
  }
}
