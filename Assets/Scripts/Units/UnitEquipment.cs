using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEquipment : MonoBehaviour {
  private Unit unit;
  private SkinnedMeshRenderer body;
  private CapsuleCollider[] clothColliders = {};

  public Weapon primary;
  public Equipment secondary;
  public Armor armor;
  public AdditionalItem additional;

  public Transform rightHand;
  public Transform leftHand;
  public Transform beard;
  public Transform hair;

  private readonly float damageScalingFactor = 0.75f;

  private void Awake() {
    unit = transform.GetComponent<Unit>();
    body = transform.Find("Model/Body").GetComponent<SkinnedMeshRenderer>();

    CapsuleCollider hipsCollider = transform.Find("Model/Armature/mixamorig:Hips").GetComponent<CapsuleCollider>();
    CapsuleCollider spineCollider = transform.Find("Model/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2").GetComponent<CapsuleCollider>();

    if (hipsCollider != null && spineCollider != null) {
      clothColliders = new CapsuleCollider[] {
        hipsCollider, spineCollider
      };
    }

    if (!ComponentsInitialized()) {
      Debug.LogError("Unit equipment components initialization error");
    }
  }

  private void Start() {
    if (!ComponentsInitialized()) return;
    UpdateEquipment();
  }

  private bool ComponentsInitialized() {
    return new object[] {
      unit, body, rightHand, leftHand
    }.All(x => x != null);
  }

  private void UpdateEquipment() {
    if (armor != null) {
      GameObject prefab = unit.Size == ArmorSize.L
        ? armor.prefabL
        : armor.prefabM;

      if (prefab != null) {
        GameObject armorObj = Instantiate(prefab);
        Transform model = armorObj.transform.Find("Model");
        Transform cape = armorObj.transform.Find("Cape");

        if (cape != null) {
          if (cape.TryGetComponent<SkinnedMeshRenderer>(out var capeMesh)) RetargetBones(capeMesh);
          if (cape.TryGetComponent<Cloth>(out var cloth)) cloth.capsuleColliders = clothColliders;
        }

        if (model != null && model.TryGetComponent<SkinnedMeshRenderer>(out var armorMesh)) {
          RetargetBones(armorMesh);
          if (hair != null) hair.gameObject.SetActive(!armor.bodyView.hideHair);
          if (beard != null) beard.gameObject.SetActive(!armor.bodyView.hideBeard);
          UpdateMaterials();
        }
      }
    }

    foreach (Transform item in rightHand) {
      if (item.gameObject.CompareTag("FakeObject")) continue;
      Destroy(item.gameObject);
    }

    foreach (Transform item in leftHand) {
      if (item.gameObject.CompareTag("FakeObject")) continue;
      Destroy(item.gameObject);
    }

    if (primary != null) {
      Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + primary.name);
      if (loadedWeapon == null) return;
      Transform hand = loadedWeapon.type == EquipmentType.Bow ? leftHand : rightHand;
      GameObject weaponObj = Instantiate(loadedWeapon.prefab, hand);
      weaponObj.transform.SetParent(hand, false);
    }

    if (secondary != null) {
      if (secondary is Weapon secWeapon) {
        Weapon loaded = Resources.Load<Weapon>("Weapon/" + secondary.name);
        if (loaded == null) return;
        GameObject obj = Instantiate(loaded.prefab, leftHand);
        obj.transform.SetParent(leftHand, false);
      } else if (secondary is Armor secArmor) {
        Armor loaded = Resources.Load<Armor>("Armor/" + secondary.name);
        if (loaded == null) return;
        GameObject obj = Instantiate(loaded.prefabM, leftHand);
        obj.transform.SetParent(leftHand, false);
      }
    }
  }

  private void RetargetBones(SkinnedMeshRenderer mesh) {
    Dictionary<string, Transform> boneMap = new ();
    foreach (Transform bone in body.bones) boneMap[bone.name] = bone;
    Transform[] armorBones = new Transform[mesh.bones.Length];

    for (int i = 0; i < mesh.bones.Length; i++) {
      string boneName = mesh.bones[i].name;
      armorBones[i] = boneMap[boneName];
    }

    mesh.bones = armorBones;
    mesh.rootBone = body.rootBone;
  }

  private void UpdateMaterials() {
    Material[] mats = body.sharedMaterials;
    BodyView bv = armor.bodyView;

    Material[] materials = {
      bv.torsoMaterial,
      bv.underwearMaterial,
      bv.legsMaterial,
      bv.footsMaterial,
      bv.armsMaterial,
      bv.handsMaterial
    };

    for (int i = 0; i < materials.Length; i++) {
      if (materials[i] == null) continue;
      mats[i] = materials[i];
    }

    body.sharedMaterials = mats;
  }

  public void EquipItem(Equipment item) {
    List<Equipment> inventory = Player.Instance.Inventory.Equip;
    if (!inventory.Contains(item)) return;
    Equipment oldItem = null;

    switch (item) {
      case Weapon newWeapon:
        switch (newWeapon.slot) {
          case UnitEquipSlot.Primary:
            oldItem = primary;
            primary = newWeapon;
            break;
          // FIXME: Может быть не только оружие
          case UnitEquipSlot.Secondary:
            oldItem = secondary;
            secondary = newWeapon;
            break;
        }
        break;

      case Armor newArmor:
        switch (newArmor.slot) {
          case UnitEquipSlot.Armor:
            oldItem = armor;
            armor = newArmor;
            break;
          case UnitEquipSlot.Secondary:
            oldItem = secondary;
            secondary = newArmor;
            break;
        }
        break;

      case AdditionalItem newAdditional:
        oldItem = additional;
        additional = newAdditional;
        break;
    }

    inventory.Remove(item);
    if (oldItem != null) inventory.Add(oldItem);
    if (unit.IsHero) Player.Instance.Inventory.UpdateEquipment();
    Player.Instance.Army.UpdateState();
    Player.Instance.Inventory.UpdateState();
  }

  public void UnequipAll() {
    List<Equipment> inventory = Player.Instance.Inventory.Equip;
    inventory.Add(primary);
    inventory.Add(armor);
    if (secondary != null) inventory.Add(secondary);
    if (additional != null) inventory.Add(additional);

    primary = null;
    secondary = null;
    armor = null;
    additional = null;

    Player.Instance.Army.UpdateState();
    Player.Instance.Inventory.UpdateState();
  }

  // Getters
  public List<Equipment> GetEquipmentList(MenuFilter filter = MenuFilter.All) {
    List<Equipment> result = new() { primary, armor };
    if (secondary != null) result.Add(secondary);
    if (additional != null) result.Add(additional);
    return result
      .Where(e => {
        if (
          (filter == MenuFilter.Weapon && e is not Weapon) ||
          (filter == MenuFilter.Armor && e is not Armor) ||
          (filter == MenuFilter.Additional && e is not AdditionalItem)
        ) return false;
        return true;
      })
      .ToList();
  }

  public float GetTotalDefense() {
    float result = 0;

    // Equipment
    if (armor != null) result += armor.defense;
    if (secondary != null) {
      if (secondary is Armor secArmor) result += secArmor.defense;
    }

    // Supports
    float supportBonus = SupportController.GetBonus("su5", relation: unit.Relation)[0];
    if (supportBonus > 0) result *= 1f + (supportBonus / 100);

    return result;
  }

  public Dictionary<DamageType, float> GetTotalResists() {
    Dictionary<DamageType, float> result = new(armor.resists);

    // Equipment
    if (secondary != null) {
      if (secondary is Armor secArmor) {
        foreach (var dmg in secArmor.resists) {
          if (result.ContainsKey(dmg.Key)) result[dmg.Key] += dmg.Value;
          else result[dmg.Key] = dmg.Value;
        }
      }
    }
    return result;
  }

  public float GetTotalDamage() {
    float result = primary.damage;

    // Buffs
    if (unit.Relation == UnitRelation.Ally && StateManager.playerBuffs.Contains("b1"))
      result += 1;

    // Supports
    float supportBonus = SupportController.GetBonus("su4", relation: unit.Relation)[0];
    if (supportBonus > 0) result *= 1f + (supportBonus / 100);

    // Core stats
    foreach (CoreStat stat in primary.scalingStats) {
      switch (stat) {
        case CoreStat.Strength: result += unit.Strength * damageScalingFactor; break;
        case CoreStat.Dexterity: result += unit.Dexterity * damageScalingFactor; break;
        case CoreStat.Intelligence: result += unit.Intelligence * damageScalingFactor; break;
      }
    }

    return result;
  }

  public float GetRange() {
    return primary.range;
  }

  public int GetWeightCoefficient() {
    int result = 0;
    if (primary != null) result += GetWeightValue(primary.weight);
    if (secondary != null) result += GetWeightValue(secondary.weight);
    if (armor != null) result += GetWeightValue(armor.weight);
    if (additional != null) result += GetWeightValue(additional.weight);
    return result;
  }

  private int GetWeightValue(EquipmentWeight weight) {
    return weight switch {
      EquipmentWeight.Medium => 1,
      EquipmentWeight.Heavy => 2,
      _ => 0
    };
  }

  // Skills
  public List<Skill> GetActiveSkills() {
    return GetSkills(true);
  }

  public List<Skill> GetPassiveSkills() {
    return GetSkills(false);
  }

  private List<Skill> GetSkills(bool active) {
    return new[] { primary, secondary, armor, additional }
      .Where(e => e != null && e.skills != null)
      .SelectMany(e => e.skills)
      .Concat(unit.Effects.innateSkills)
      .Where(s => s != null && s.isActive == active)
      .ToList();
  }

  public bool HasAttackPhaseSkills() {
    if (unit.SkillCharges == 0) return false;
    foreach (Skill skill in GetActiveSkills()) {
      // FIXME: Может сломаться проверка, если использовать не в PhaseManager
      if (skill.skillName == "Charged attack") continue;
      if (unit.Effects.HasAnyEffect(new string[] { "Stun", "Root" }) && !skill.canUseInRoot) continue;
      if (skill.skillPhases.Contains(BattlePhase.Attack)) return true;
    }
    return false;
  }

  public void ApplyInstantEffects() {
    List<Skill> skills = GetPassiveSkills();
    if (skills.Count == 0) return;

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case "Inspiration":
          Effect effect = Factory.CreateEffectById("e4");
          if (effect != null) unit.Effects.ApplyEffect(effect);
          break;
      }
    }
  }

  // Capabilities
  public bool CanBreakObjects() {
    return primary.damageType == DamageType.Chop || primary.damageType == DamageType.Crash;
  }

  public bool CanChopTrees() {
    return primary.damageType == DamageType.Chop;
  }

  public int CanEquip(Equipment item, UnitEquipSlot slot) {
    int result = -1;
    if (item.slot != slot) return result;

    switch (slot) {
      case UnitEquipSlot.Primary:
        if (item is Weapon weapon1) {
          if (unit.AllowedWeapon == weapon1.type) result = 0;
        }
        break;
      case UnitEquipSlot.Armor:
        // FIXME: Проверить возможность носить броню
        break;
      case UnitEquipSlot.Secondary:
        if (item is Armor armor2) {
          if (armor2.type == EquipmentType.Shield && unit.ShieldIsAllow) result = 0;
        }
        break;
      case UnitEquipSlot.Additional:
        if (item is AdditionalItem additional) {
          if (
            additional.unitTypes.Contains(unit.Type) &&
            additional.allowedWeapons.Contains(unit.AllowedWeapon)
          ) result = 0;
        }
        break;
    }

    if (result < 0) return result;

    float[] unitStats = { unit.Strength, unit.Dexterity, unit.Intelligence };

    bool enoughStats = true;
    for (int i = 0; i < item.GetRequirementStats().Length; i++) {
      if (item.GetRequirementStats()[i] > unitStats[i]) {
        enoughStats = false;
        break;
      }
    }
    if (item.requirementLevel <= unit.Level && enoughStats) result = 1;

    return result;
  }

  public bool HasItem(Equipment item) {
    return new Equipment[] { primary, secondary, armor, additional }
      .Any(e => e != null && e.id == item.id);
  }
}
