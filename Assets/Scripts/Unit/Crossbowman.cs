using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Crossbowman : Unit
{
  private Crossbowman() {
    Strength = 3;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Crossbowman";
    Description = "Extremely immobile, well-protected shooter. Effective at medium ranges.";
    prefabId = 4;
    Type = UnitType.Range;
    AllowedWeapon = EquipmentType.Crossbow;
    TotalHealth = 20f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 4;
    Priority = 15;
  }

  public GameObject boltPrefab;
  private Transform weapon;
  private Transform missleSpawner;
  private readonly int boltSpeed = 17;

  public override bool Init(Tile tile, UnitRelation relation, Vector3 direction) {
    if (!base.Init(tile, relation, direction)) return false;

    weapon = GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.CompareTag("Weapon"));
    missleSpawner = weapon.transform.Find("MissleSpawner").GetComponent<Transform>();

    if (weapon == null || missleSpawner == null) {
      Debug.LogError("Crossbowman components initialization error");
    }
    return true;
  }

  public async override void OnAttack(Unit target = null) {
    BattleUI.DisableUI();
    if (target != null) Target = target;

    Vector3 dirToTarget = (Target.transform.position - transform.position).normalized;
    Vector3 dirFromTarget = (transform.position - Target.transform.position).normalized;

    await Task.WhenAll(
      Animator.RotateTowards(dirToTarget),
      Target.Animator.RotateTowards(dirFromTarget)
    );

    Animator.Attack();
  }

  public override void Shoot() {
    Vector3 shootDirection = (Target.UnitCollider.bounds.center - missleSpawner.position).normalized;

    GameObject bolt = Instantiate(
      boltPrefab,
      missleSpawner.position,
      Quaternion.LookRotation(shootDirection)
    );

    float hitChance = BattleManager.GetHitChance(this, Target);
    float critModifier = BattleManager.GetCritModifier(this, Target);
    float damage = BattleManager.CalculateDamage(this, Target);
    successAttack = Utils.RollChance(hitChance);

    Missle missle = bolt.GetComponent<Missle>();
    missle.Launch(this, shootDirection, boltSpeed, damage, critModifier, successAttack);
  }
}
