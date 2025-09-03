using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour {
  // Components
  public CapsuleCollider UnitCollider { get; private set; }
  public UnitMove Move { get; private set; }
  public UnitHealth Health { get; private set; }
  public UnitUI Ui { get; private set; }
  public UnitAnimator Animator { get; private set; }
  public UnitEquipment Equip { get; private set; }
  public UnitEffects Effects { get; private set; }
  public Sprite avatar;
  public string PrefabId { get; protected set; }

  // Temporary
  public Tile CurrentTile { get; set; }
  public Unit Target { get; set; }
  public Breakable TargetObject { get; set; }
  public TreeObject TargetTree { get; set; }
  protected bool successAttack = true;

  // AI
  protected int Priority;
  public AIBehaviorType BehaviorType { get; set; }

  // Core stats
  public float Strength { get; protected set; } // Heavy items / Damage / Stun chance and protect
  public float Dexterity { get; protected set; } // Assassin items / Evasion / Crit chance
  public float Intelligence { get; protected set; } // Mage items / Spell damage / Mana

  // Parameters
  public string Name { get; protected set; }
  public string Description { get; protected set; }
  public bool IsHero { get; protected set; } = false;
  public bool IsBoss { get; protected set; } = false;
  public UnitType Type { get; protected set; }
  public UnitRelation Relation { get; private set; }
  public Reward killReward;
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
  public int Projectiles { get; protected set; }
  public int CurrentProjectiles { get; protected set; }

  // State
  public bool IsDead { get; set; }
  public bool InSquad { get; set; } = true;
  public bool IsNew { get; set; }

  protected void Awake() {
    Equip = transform.GetComponent<UnitEquipment>();
    if (!IsDead && CurrentHealth <= 0) CurrentHealth = TotalHealth;
  }

  private void OnDestroy() {
    CurrentTile = null;
    Target = null;
    TargetObject = null;
    TargetTree = null;
  }

  public virtual void Init(Tile tile, UnitRelation relation, Vector3 direction) {
    UnitCollider = transform.GetComponent<CapsuleCollider>();
    Move = transform.GetComponent<UnitMove>();
    Health = transform.GetComponent<UnitHealth>();
    Ui = transform.GetComponent<UnitUI>();
    Animator = transform.GetComponent<UnitAnimator>();
    Effects = transform.GetComponent<UnitEffects>();
    SetMovePoints();

    if (
      UnitCollider == null || Move == null || Health == null ||
      Ui == null || Animator == null || Equip == null ||
      Effects == null
    ) {
      Debug.LogError("Unit components initialization error");
      return;
    }

    CurrentTile = tile;
    tile.OccupiedBy = this;
    Relation = relation;

    _ = Animator.RotateTowards(direction, true);

    if (Relation == UnitRelation.Ally) Ui.MarkAsAlly();
    if (CurrentHealth <= 0) IsDead = true;

    Ui.UpdateHealth(TotalHealth, CurrentHealth);
    SkillCharges = TotalSkillCharges;
    CurrentProjectiles = Projectiles;

    if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    return;
  }

  private void SetMovePoints() {
    float result = DefaultMovePoints;
    Equipment[] unitEquip = { Equip.primary, Equip.secondary, Equip.armor };

    foreach (Equipment item in unitEquip) {
      if (item == null) continue;
      if (item.weight == EquipmentWeight.Heavy) result -= 2f;
      else if (item.weight == EquipmentWeight.Medium) result -= 1f;
    }

    if (result < 1f) result = 1f;
    if (result < 4f) MoveSpeed *= 0.9f;
    else if (result > 7f) MoveSpeed *= 1.1f;
    TotalMovePoints = result;
    CurrentMovePoints = result;
  }

  public void ResetMovePoints() {
    CurrentMovePoints = TotalMovePoints;
  }

  void OnMouseEnter() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    InfoPopup.Show(this);
    if (PhaseManager.CurrentPhase != BattlePhase.Attack) return;
    CurrentTile.Hover();
  }

  void OnMouseExit() {
    InfoPopup.Hide();
    CurrentTile.Unhover();
  }

  public float GetPriority() {
    float result = Priority;
    // FIXME: Проверка на разные защитные эффекты и условия окружения
    if (Type == UnitType.Range && CurrentProjectiles == 0) return 0;
    if (Effects.HasEffect("Cover")) result -= 2;
    if (CurrentHealth < TotalHealth / 3) result *= 2;
    else if (CurrentHealth < TotalHealth / 2) result *= 1.5f;
    if (Effects.HasEffect("Block") || Effects.HasEffect("Stun")) result /= 3;
    return result;
  }

  public void IncreaseStats(int[] stats) {
    if (stats == null || stats.Length != 3) return;
    Strength += stats[0];
    Dexterity += stats[1];
    Intelligence += stats[2];
  }

  public void AddProjectiles(int value) {
    if (Type != UnitType.Range) return;
    CurrentProjectiles += value;
    if (CurrentProjectiles > Projectiles) CurrentProjectiles = Projectiles;
  }

  // Attack
  public async virtual void OnAttack(Unit target = null) {
    // FIXME: Проверка пассивных скиллов атакующего
    BattleUI.Instance.DisableUI();
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

    Animator.SetAttackType(Equip.primary.attackType);
    Animator.Attack();
  }

  public async void BreakObject(Breakable target) {
    BattleUI.Instance.DisableUI();
    TargetObject = target;
    Vector3 dirToTarget = (TargetObject.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
    Animator.SetAttackType(Equip.primary.attackType);
    Animator.Attack();
  }

  public async void ChopTree(TreeObject target) {
    BattleUI.Instance.DisableUI();
    TargetTree = target;
    Vector3 dirToTarget = (TargetTree.ParentTile.GetPos() - transform.position).normalized;
    await Animator.RotateTowards(dirToTarget);
    Animator.SetAttackType(Equip.primary.attackType);
    Animator.Attack();
  }

  public void DealDamage() {
    if (Target != null) {
      if (successAttack) {
        if (!DamageBlocked()) {
          float critModifier = BattleManager.GetCritModifier(this, Target);
          float damage = BattleManager.CalculateDamage(this, Target);
          List<Effect> effects = BattleManager.GetItemEffects(this, Target);
          foreach (Effect effect in effects) Target.Effects.ApplyEffect(effect);
          Target.Health.TakeDamage(damage, critModifier);
        }
      } else {
        Target.Ui.ShowPopup("Miss!");
      }
      Target = null;
    }

    if (TargetObject != null) {
      TargetObject.Break();
      _ = CameraController.Shake(0.8f);
      TargetObject = null;
      FinishAction();
    }

    if (TargetTree != null) {
      TargetTree.Chop();
      _ = CameraController.Shake(0.8f);
      TargetTree = null;
      FinishAction();
    }
  }

  private bool DamageBlocked() {
    List<Skill> skills = BattleManager.GetItemPassiveSkills(Target);

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case SkillName.Parry:
          Target.Animator.Parry();
          Target.Ui.ShowPopup("Parry!");
          Target.Health.TakeDamage(0f, 1f);
          return true;
      }
    }

    if (Target.Effects.HasEffect("Wall")) {
      if (
        Equip.primary.damageType == DamageType.Chop ||
        Equip.primary.damageType == DamageType.Crash
      ) {
        bool isBreak = Utils.RollChance(Equip.primary.armorPenetration);
        if (isBreak) {
          Target.Effects.ClearEffect("Wall");
          return false;
        }
      }

      Target.Ui.ShowPopup("Block!");
      Target.Health.TakeDamage(0f, 1f);
      return true;
    }

    return false;
  }

  public void FinishAction() {
    PhaseManager.NextPhase();
  }

  // Data transfer
  public UnitData ToData() {
    float health = CurrentHealth;
    if (!IsDead && CurrentHealth <= 0) health = TotalHealth;
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    return new UnitData {
      prefabId = PrefabId,
      currentHealth = health,
      inSquad = InSquad,
      isBoss = IsBoss,
      strength = Strength,
      dexterity = Dexterity,
      intelligence = Intelligence,
      level = Level,
      type = Type,
      primaryId = equipment.primary != null ? equipment.primary.id : null,
      secondaryId = equipment.secondary != null ? equipment.secondary.id : null,
      armorId = equipment.armor != null ? equipment.armor.id : null,
    };
  }

  public void FromData(UnitData data) {
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    Weapon primary = Factory.CreateEquipById(data.primaryId) as Weapon;
    Equipment secondary = Factory.CreateEquipById(data.secondaryId);
    Armor armor = Factory.CreateEquipById(data.armorId) as Armor;

    CurrentHealth = data.currentHealth;
    InSquad = data.inSquad;
    Strength = data.strength;
    Dexterity = data.dexterity;
    Intelligence = data.intelligence;
    Level = data.level;
    equipment.primary = primary;
    equipment.secondary = secondary;
    equipment.armor = armor;
  }

  // Overloaded
  public virtual void Shoot() {
    CurrentProjectiles -= 1;
    if (CurrentProjectiles == 0) BehaviorType = AIBehaviorType.Retreat;
  }

  public virtual void BlockStance(SkillName type) { }
}
