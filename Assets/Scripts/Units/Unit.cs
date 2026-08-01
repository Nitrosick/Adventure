using System.Collections.Generic;
using System.Linq;
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
  public EquipmentType[] AllowedWeapon { get; protected set; }
  public ArmorSize Size { get; protected set; } = ArmorSize.M;

  public int Level { get; protected set; } = 1;
  public int MaxLevel { get; protected set; }
  public CoreStat LevelingCoreStat { get; protected set; }
  public int Initiative { get; protected set; }
  public float MoveSpeed { get; protected set; }
  public float DefaultMovePoints { get; protected set; }
  public float TotalMovePoints { get; protected set; }
  public float CurrentMovePoints { get; set; }
  public float TotalHealth { get; protected set; }
  public float CurrentHealth { get; set; }
  public int TotalSkillCharges { get; protected set; } = 3;
  public int SkillCharges { get; protected set; }
  public int Projectiles { get; protected set; }
  public int CurrentProjectiles { get; protected set; }

  // State
  public bool IsDead { get; set; }
  public bool InSquad { get; set; }
  public bool IsNew { get; set; }
  public int FailedAttacks { get; set; }
  public AttackType CurrentAttackType { get; set; }

  protected void Awake() {
    Health = transform.GetComponent<UnitHealth>();
    Equip = transform.GetComponent<UnitEquipment>();
    Effects = transform.GetComponent<UnitEffects>();
  }

  void OnDestroy() {
    CurrentTile = null;
    Target = null;
    TargetObject = null;
    TargetTree = null;
  }

  public virtual void Init(Tile tile, UnitRelation relation) {
    UnitCollider = transform.GetComponent<CapsuleCollider>();
    Move = transform.GetComponent<UnitMove>();
    Ui = transform.GetComponent<UnitUI>();
    Animator = transform.GetComponent<UnitAnimator>();

    SetMovePoints();
    SetProjectiles();

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
    FailedAttacks = 0;

    Ui.InitMarkersColor();
    if (CurrentHealth == 0) CurrentHealth = Health.GetMaxHP();
    Ui.UpdateHealth(Health.GetMaxHP(), CurrentHealth);

    if (IsHero) TotalSkillCharges += (int)AbilityController.ChargesBonus();
    SetSkillCharges(TotalSkillCharges);
    if (Equip.GetActiveSkills().Count > 0) Ui.UpdateCharges(TotalSkillCharges, SkillCharges);
    Equip.ApplyInstantEffects();
  }

  private void SetMovePoints() {
    float result = DefaultMovePoints;
    Equipment[] unitEquip = { Equip.primary, Equip.secondary, Equip.armor };

    // Weight
    foreach (Equipment item in unitEquip) {
      if (item == null) continue;
      if (item.weight == EquipmentWeight.Heavy) result -= 2f;
      else if (item.weight == EquipmentWeight.Medium) result -= 1f;
    }

    // Passive skills
    List<Skill> skills = Equip.GetPassiveSkills();
    foreach (Skill skill in skills) {
      if (skill.skillName == "Comfort") result++;
    }

    // Supports
    float supportBonus = SupportController.GetBonus("su6", relation: Relation, targetUnit: this)[0];
    result += supportBonus;

    if (result < 1f) result = 1f;
    if (result < 4f) MoveSpeed *= 0.9f;
    else if (result > 7f) MoveSpeed *= 1.1f;
    TotalMovePoints = result;
    CurrentMovePoints = result;
  }

  private void SetProjectiles() {
    if (Type != UnitType.Range) return;

    // Additional equipment
    AdditionalItem item = Equip.additional;
    if (item != null && item.bonusType == ItemBonus.AdditionalProjectiles) {
      Projectiles += (int)item.bonusValue;
    }

    // Supports
    float supportBonus = SupportController.GetBonus("su6", relation: Relation)[1];
    Projectiles += (int)supportBonus;

    CurrentProjectiles = Projectiles;
  }

  public void SetSkillCharges(int val) {
    SkillCharges += val;
    if (SkillCharges < 0) SkillCharges = 0;
    else if (SkillCharges > TotalSkillCharges) SkillCharges = TotalSkillCharges;
  }

  public void SetAttackType(AttackType attackType) {
    CurrentAttackType = attackType;
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
    // TODO: Проверка на разные защитные эффекты и условия окружения
    if (Type == UnitType.Range && CurrentProjectiles == 0) return 0;

    float result = Priority;

    // Items
    if (Equip.additional != null) {
      if (Equip.additional.id == "ai4") result += Equip.additional.bonusValue;
    }

    // Abilities
    if (IsHero) result += AbilityController.AttackPriorityBonus();

    // Health
    if (CurrentHealth < Health.GetMaxHP() / 3) result *= 2;
    else if (CurrentHealth < Health.GetMaxHP() / 2) result *= 1.5f;

    // Effects
    if (Effects.HasEffect("Cover")) result -= 2;
    if (Effects.HasAnyEffect(new string[] { "Block", "Stun" })) result /= 3;

    return result;
  }

  public int GetInitiative() {
    int result = Initiative;

    // Buffs
    if (Relation == UnitRelation.Ally && StateManager.playerBuffs.Contains("b1"))
      result += 1;

    // Abilities
    if (Relation == UnitRelation.Ally)
      result += (int)AbilityController.MovePriorityBonus();

    // Effects
    if (
      QueueManager.Instance.Queue
        .Where(u => u.Relation == Relation && u != this)
        .Any(u => u.Effects.HasEffect("Inspiration"))
    ) result += 2;

    return result;
  }

  public float GetValue() {
    float typeBonus = Type == UnitType.Range || Type == UnitType.Mage ? 2 : 0;
    float bossBonus = IsBoss ? 3 : 0;
    return (TotalHealth / 10) + Strength + Dexterity + Intelligence + (Level * 5) + (DefaultMovePoints / 2) + typeBonus + bossBonus;
  }

  public void LevelUp(int value = 1) {
    if (Level >= MaxLevel) return;
    int levels = value;

    while (levels > 0) {
      if (Level >= MaxLevel) {
        Player.Instance.Army.UpdateState();
        return;
      }

      Level++;
      if (!IsHero) {
        switch (LevelingCoreStat) {
          case CoreStat.Strength: Strength++; break;
          case CoreStat.Dexterity: Dexterity++; break;
          case CoreStat.Intelligence: Intelligence++; break;
        }
      }
      levels--;
    }

    Player.Instance.Army.UpdateState();
  }

  public void IncreaseStats(int[] stats) {
    if (stats == null || stats.Length != 3) return;
    Strength += stats[0];
    Dexterity += stats[1];
    Intelligence += stats[2];
    Player.Instance.Army.UpdateState();
  }

  public void AddProjectiles(int value) {
    if (Type != UnitType.Range) return;
    CurrentProjectiles += value;
    if (CurrentProjectiles > Projectiles) CurrentProjectiles = Projectiles;
  }

  // Data transfer
  public UnitData ToData() {
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    return new UnitData {
      prefabId = PrefabId,
      currentHealth = CurrentHealth,
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
      additionalId = equipment.additional != null ? equipment.additional.id : null,
    };
  }

  public void FromData(UnitData data) {
    UnitEquipment equipment = transform.GetComponent<UnitEquipment>();

    Weapon primary = Factory.CreateEquipById(data.primaryId) as Weapon;
    Equipment secondary = Factory.CreateEquipById(data.secondaryId);
    Armor armor = Factory.CreateEquipById(data.armorId) as Armor;
    AdditionalItem additional = Factory.CreateEquipById(data.additionalId) as AdditionalItem;

    CurrentHealth = data.currentHealth;
    InSquad = data.inSquad;
    Strength = data.strength;
    Dexterity = data.dexterity;
    Intelligence = data.intelligence;
    Level = data.level;
    equipment.primary = primary;
    equipment.secondary = secondary;
    equipment.armor = armor;
    equipment.additional = additional;
  }

  // Overloaded
  public virtual void OnAttack(Unit target = null) { }
  public virtual void DealDamage(bool charged = false) { }
  public virtual void DealPierceDamage(bool charged = false) { }
  public virtual void DealAoeDamage(AttackType attackType) { }

  public virtual void BreakObject(Breakable target) { }
  public virtual void ChopTree(TreeObject target) { }

  public virtual void Shoot() { }
  public virtual void CrossbowShoot() { }
  public virtual void BowShoot() { }

  public virtual void BlockStance(string id) { }
  protected virtual bool DamageBlocked() { return false; }

  public virtual void NextPhase(bool instant = false) { }
  protected virtual void LogDamage(float damage, float critModifier, List<Effect> effects) { }
}
