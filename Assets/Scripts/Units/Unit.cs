using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour {
  // Components
  public CapsuleCollider UnitCollider { get; private set; }
  public UnitUI Ui { get; private set; }
  public UnitAnimator Animator { get; private set; }
  public UnitEquipment Equip { get; private set; }
  public UnitEffects Effects { get; private set; }
  public Sprite avatar;
  protected string prefabId;

  // Temporary
  public Tile CurrentTile { get; set; }
  public Unit Target { get; set; }
  public Breakable TargetObject { get; set; }
  protected bool successAttack = true;

  // AI
  protected int Priority;

  // Core stats
  public float Strength { get; protected set; } // Heavy items / Damage / Stun chance and protect
  public float Dexterity { get; protected set; } // Assassin items / Evasion / Crit chance
  public float Intelligence { get; protected set; } // Mage items / Spell damage / Mana

  // Parameters
  public string Name { get; protected set; }
  public string Description { get; protected set; }
  public bool IsHero { get; protected set; } = false;
  public UnitType Type { get; protected set; }
  public UnitRelation Relation { get; private set; }
  public BattleReward killReward;
  public EquipmentType AllowedWeapon { get; protected set; }
  public bool ShieldIsAllow { get; protected set; } = false;

  public int Level { get; protected set; } = 1;
  public int Initiative { get; protected set; }
  public float MoveSpeed { get; protected set; }
  public float DefaultMovePoints { get; protected set; }
  public float TotalMovePoints { get; protected set; }
  public float CurrentMovePoints { get; set; }
  public float TotalHealth { get; protected set; }
  public float CurrentHealth { get; set; }
  // FIXME: Управление зарядами скиллов
  public int TotalSkillCharges { get; protected set; } = 3;
  public int SkillCharges { get; protected set; }

  protected readonly int objectDestroyTime = 10;

  // State
  public bool IsDead { get; protected set; }
  public bool InSquad { get; set; } = true;

  protected void Awake() {
    Equip = transform.GetComponent<UnitEquipment>();
    if (!IsDead && CurrentHealth <= 0) CurrentHealth = TotalHealth;
  }

  private void OnDestroy() {
    CurrentTile = null;
    Target = null;
    TargetObject = null;
  }

  public virtual bool Init(Tile tile, UnitRelation relation, Vector3 direction) {
    UnitCollider = transform.GetComponent<CapsuleCollider>();
    Ui = transform.GetComponent<UnitUI>();
    Animator = transform.GetComponent<UnitAnimator>();
    Effects = transform.GetComponent<UnitEffects>();
    SetMovePoints();

    if (UnitCollider == null || Ui == null || Animator == null || Equip == null || Effects == null) {
      Debug.LogError("Unit components initialization error");
      return false;
    }

    CurrentTile = tile;
    tile.OccupiedBy = this;
    Relation = relation;

    _ = Animator.RotateTowards(direction, true);

    if (Relation == UnitRelation.Ally) Ui.MarkAsAlly();
    if (CurrentHealth <= 0) IsDead = true;
    Ui.UpdateHealth(TotalHealth, CurrentHealth);
    SkillCharges = TotalSkillCharges;
    if (Equip.GetSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    return true;
  }

  private void SetMovePoints() {
    float result = DefaultMovePoints;
    Equipment[] unitEquip = { Equip.primaryWeapon, Equip.secondaryWeapon, Equip.shield, Equip.armor };

    foreach (Equipment item in unitEquip) {
      if (item == null) continue;
      if (item.weight == EquipmentWeight.Heavy) result -= 2f;
      else if (item.weight == EquipmentWeight.Medium) result -= 1f;
    }

    if (result < 1f) result = 1f;
    TotalMovePoints = result;
    CurrentMovePoints = result;
  }

  public void ResetMovePoints() {
    CurrentMovePoints = TotalMovePoints;
  }

  void OnMouseEnter() {
    if (EventSystem.current.IsPointerOverGameObject()) return;

    BattleUI.ShowUnitInfo(
      Name,
      Description,
      new float[] { Strength, Dexterity, Intelligence },
      TotalMovePoints,
      TotalHealth,
      CurrentHealth,
      Equip.primaryWeapon.damage,
      Equip.GetTotalDefense(),
      Equip.primaryWeapon.range,
      Effects.ActiveEffects
    );

    if (PhaseManager.CurrentPhase != BattlePhase.Attack) return;
    CurrentTile.Hover();
  }

  void OnMouseExit() {
    BattleUI.HideUnitInfo();
    CurrentTile.Unhover();
  }

  public float GetPriority() {
    float result = Priority;
    // FIXME: Проверка на разные защитные эффекты и условия окружения
    if (Effects.HasEffect("Cover")) result -= 2;
    if (CurrentHealth < TotalHealth / 3) result *= 2;
    else if (CurrentHealth < TotalHealth / 2) result *= 1.5f;
    if (Effects.HasEffect("Block")) result /= 3;
    return result;
  }

  public void IncreaseStats(int[] stats) {
    if (stats == null || stats.Length != 3) return;
    Strength += stats[0];
    Dexterity += stats[1];
    Intelligence += stats[2];
  }

  // Attack and damage
  public async virtual void OnAttack(Unit target = null) {
    BattleUI.DisableUI();
    if (target != null) Target = target;

    Vector3 dirToTarget = (Target.transform.position - transform.position).normalized;
    Vector3 dirFromTarget = (transform.position - Target.transform.position).normalized;

    await Task.WhenAll(
      Animator.RotateTowards(dirToTarget),
      Target.Animator.RotateTowards(dirFromTarget)
    );

    float hitChance = BattleManager.GetHitChance(this, Target);
    successAttack = Utils.RollChance(hitChance);

    if (!successAttack) Target.Animator.Dodge();

    Animator.SetAttackType(Equip.primaryWeapon.attackType);
    Animator.Attack();
  }

  public async void BreakObject(Breakable target) {
    BattleUI.DisableUI();
    TargetObject = target;

    Vector3 dirToTarget = (TargetObject.transform.position - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);

    Animator.SetAttackType(Equip.primaryWeapon.attackType);
    Animator.Attack();
  }

  public void DealDamage() {
    if (Target != null) {
      if (successAttack) {
        float critModifier = BattleManager.GetCritModifier(this, Target);
        float damage = BattleManager.CalculateDamage(this, Target);
        Effect effect = BattleManager.GetWeaponEffect(this, Target);
        if (effect != null) Target.Effects.ApplyEffect(effect);
        Target.TakeDamage(damage, critModifier);
      } else {
        Target.Ui.ShowPopup("Miss!");
      }
      Target = null;
    }

    if (TargetObject != null) {
      TargetObject.Break();
      TargetObject = null;
      FinishAction();
    }
  }

  public void TakeDamage(float damage, float modifier, bool isTickDamage = false) {
    float totalDamage = damage * modifier;

    if (modifier > 1f) {
      Ui.ShowPopup(totalDamage.ToString(), PopupType.Crit);
      if (!isTickDamage) CameraController.Shake(1.2f);
    } else {
      Ui.ShowPopup(totalDamage.ToString(), PopupType.Negative);
      if (!isTickDamage) CameraController.Shake(0.8f);
    }

    if (totalDamage >= CurrentHealth) {
      CurrentHealth = 0;
      Die();
    } else {
      CurrentHealth -= totalDamage;
      Ui.UpdateHealth(TotalHealth, CurrentHealth);
      if (!isTickDamage) Animator.TakeDamage();
    }
  }

  private void Die() {
    IsDead = true;
    CurrentTile.OccupiedBy = null;
    UnitCollider.enabled = false;
    Ui.ClearMarkers();
    Ui.HideHealthBar();
    Effects.ClearEffects();
    Animator.Die();
    _ = MakeCorpse();
  }

  private async Task MakeCorpse() {
    await Task.Delay(objectDestroyTime * 1000);

    GameObject model = transform.Find("Model").gameObject;
    Destroy(model);

    Instantiate(
      BattleManager.Instance.corpsePrefab,
      transform.position + new Vector3(-0.5f, 0, -0.5f),
      Quaternion.Euler(0, 65, 0)
    );
  }

  public void FinishAction() {
    PhaseManager.NextPhase();
  }

  // Data transfer
  public UnitData ToData() {
    float health = CurrentHealth;
    if (!IsDead && CurrentHealth <= 0) health = TotalHealth;
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    // FIXME: Добавить все сериализуемые поля
    return new UnitData {
      prefabId = prefabId,
      currentHealth = health,
      inSquad = InSquad,
      strength = Strength,
      dexterity = Dexterity,
      intelligence = Intelligence,
      level = Level,
      primaryWeaponId = equipment.primaryWeapon != null ? equipment.primaryWeapon.id : null,
      secondaryWeaponId = equipment.secondaryWeapon != null ? equipment.secondaryWeapon.id : null,
      shieldId = equipment.shield != null ? equipment.shield.id : null,
      armorId = equipment.armor != null ? equipment.armor.id : null,
    };
  }

  public void FromData(UnitData data) {
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    Weapon primaryWeapon = Factory.CreateById(data.primaryWeaponId) as Weapon;
    Weapon secondaryWeapon = Factory.CreateById(data.secondaryWeaponId) as Weapon;
    Armor shield = Factory.CreateById(data.shieldId) as Armor;
    Armor armor = Factory.CreateById(data.armorId) as Armor;

    CurrentHealth = data.currentHealth;
    InSquad = data.inSquad;
    Strength = data.strength;
    Dexterity = data.dexterity;
    Intelligence = data.intelligence;
    Level = data.level;
    equipment.primaryWeapon = primaryWeapon;
    equipment.secondaryWeapon = secondaryWeapon;
    equipment.shield = shield;
    equipment.armor = armor;
  }

  // Overloaded
  public virtual void Shoot() { }
  public virtual void Block() { }
}
