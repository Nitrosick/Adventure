using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEquipment : MonoBehaviour {
  private Unit unit;
  public Weapon primary;
  public Equipment secondary;
  public Armor armor;
  public AdditionalItem additional;

  private ArmorSet[] armorSets;
  public Transform rightHand;
  public Transform leftHand;
  private Transform beard;
  private Transform hair;

  private readonly float damageScalingFactor = 0.75f;

  private void Awake() {
    unit = transform.GetComponent<Unit>();
    armorSets = GetComponentsInChildren<ArmorSet>(true);
    beard = transform.Find("Model/Beard");
    hair = transform.Find("Model/Hair");

    if (unit == null || armorSets.Length == 0) {
      Debug.LogError("Unit equipment components initialization error");
    }
  }

  private void Start() {
    UpdateEquipment();
  }

  private void UpdateEquipment() {
    if (beard != null) beard.gameObject.SetActive(false);
    if (hair != null) hair.gameObject.SetActive(false);

    foreach (ArmorSet set in armorSets) {
      if (set.id == armor.id) {
        set.gameObject.SetActive(true);
        if (beard != null) beard.gameObject.SetActive(!armor.hideBeard);
        if (hair != null) hair.gameObject.SetActive(!armor.hideHair);
      }
      else set.gameObject.SetActive(false);
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
      }
      else if (secondary is Armor secArmor) {
        Armor loaded = Resources.Load<Armor>("Armor/" + secondary.name);
        if (loaded == null) return;
        GameObject obj = Instantiate(loaded.prefab, leftHand);
        obj.transform.SetParent(leftHand, false);
      }
    }
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
    if (armor != null) result += armor.defense;
    if (secondary != null) {
      if (secondary is Armor secArmor) result += secArmor.defense;
    }
    return result;
  }

  public Dictionary<DamageType, float> GetTotalResists() {
    Dictionary<DamageType, float> result = new(armor.resists);

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

  public List<Skill> GetActiveSkills() {
    return GetSkills(true);
  }

  public List<Skill> GetPassiveSkills() {
    return GetSkills(false);
  }

  private List<Skill> GetSkills(bool active) {
    return new[] { primary, secondary, armor, additional }
      .Where(e => e != null && e.skills != null && e.skills.Length > 0)
      .SelectMany(e => e.skills)
      .Where(s => s != null && s.isActive == active)
      .ToList();
  }

  public bool HasAttackPhaseSkills() {
    if (unit.SkillCharges == 0) return false;
    foreach (Skill skill in GetActiveSkills()) {
      if (unit.Effects.HasAnyEffect(new string[] { "Stun", "Root" }) && !skill.canUseInRoot) continue;
      if (skill.skillPhases.Contains(BattlePhase.Attack)) return true;
    }
    return false;
  }

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
        if (item is Armor armor1) {
          foreach (ArmorSet set in armorSets) {
            if (set.id == armor1.id) result = 0;
          }
        }
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
