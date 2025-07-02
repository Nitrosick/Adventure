using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEquipment : MonoBehaviour {
  private Unit unit;
  public Weapon primaryWeapon;
  public Weapon secondaryWeapon;
  public Armor shield;
  public Armor armor;

  private ArmorSet[] armorSets;
  public Transform rightHand;
  public Transform leftHand;

  private void Awake() {
    unit = transform.GetComponent<Unit>();
    armorSets = GetComponentsInChildren<ArmorSet>();

    if (unit == null || armorSets.Length == 0) {
      Debug.LogError("Unit equipment components initialization error");
      return;
    }

    UpdateEquipment();
  }

  private void UpdateEquipment() {
    foreach (ArmorSet set in armorSets) {
      if (set.id == armor.id) set.gameObject.SetActive(true);
      else set.gameObject.SetActive(false);
    }

    foreach (Transform item in rightHand) { Destroy(item.gameObject); }
    foreach (Transform item in leftHand) { Destroy(item.gameObject); }

    if (primaryWeapon != null) {
      Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + primaryWeapon.name);
      if (loadedWeapon == null) return;
      GameObject weaponObj = Instantiate(loadedWeapon.prefab, rightHand);
      weaponObj.transform.SetParent(rightHand, false);
    }

    if (shield != null) {
      Armor loadedShield = Resources.Load<Armor>("Armor/" + shield.name);
      if (loadedShield == null) return;
      GameObject shieldObj = Instantiate(loadedShield.prefab, leftHand);
      shieldObj.transform.SetParent(leftHand, false);
    }

    // FIXME: Обновление доп. предмета
  }

  public void EquipItem(Equipment item) {
    List<Equipment> inventory = Player.Instance.Inventory.Equip;
    if (!inventory.Contains(item)) return;
    Equipment oldItem = null;

    switch (item) {
      case Weapon newWeapon:
        switch (newWeapon.slot) {
          case UnitEquipSlot.Primary:
            oldItem = primaryWeapon;
            primaryWeapon = newWeapon;
            break;
          case UnitEquipSlot.Secondary:
            oldItem = secondaryWeapon;
            secondaryWeapon = newWeapon;
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
            oldItem = shield;
            shield = newArmor;
            break;
        }
        break;
    }

    inventory.Remove(item);
    if (oldItem != null) inventory.Add(oldItem);
  }

  public void UnequipAll() {
    List<Equipment> inventory = Player.Instance.Inventory.Equip;
    inventory.Add(primaryWeapon);
    inventory.Add(armor);
    if (secondaryWeapon != null) inventory.Add(secondaryWeapon);
    if (shield != null) inventory.Add(shield);

    primaryWeapon = null;
    secondaryWeapon = null;
    armor = null;
    shield = null;
  }

  public List<Equipment> GetEquipmentList() {
    List<Equipment> result = new() { primaryWeapon, armor };
    if (secondaryWeapon != null) result.Add(secondaryWeapon);
    if (shield != null) result.Add(shield);
    return result;
  }

  public float GetTotalDefense() {
    float result = 0;
    if (armor != null) result += armor.defense;
    if (shield != null) result += shield.defense;
    return result;
  }

  public float GetTotalDamage() {
    // FIXME: Учет предмета во второй руке
    float result = primaryWeapon.damage;
    foreach (CoreStat stat in primaryWeapon.scalingStats) {
      switch (stat) {
        case CoreStat.Strength: result += unit.Strength; break;
        case CoreStat.Dexterity: result += unit.Dexterity; break;
        case CoreStat.Intelligence: result += unit.Intelligence; break;
      }
    }
    return result;
  }

  public List<Skill> GetSkills() {
    List<Skill> result = new() { };
    if (primaryWeapon != null && primaryWeapon.skill != null) result.Add(primaryWeapon.skill);
    if (secondaryWeapon != null && secondaryWeapon.skill != null) result.Add(secondaryWeapon.skill);
    if (shield != null && shield.skill != null) result.Add(shield.skill);
    if (armor != null && armor.skill != null) result.Add(armor.skill);
    return result;
  }

  public bool HasAttackPhaseSkills() {
    if (unit.SkillCharges <= 0) return false;
    foreach (Skill skill in GetSkills()) {
      if (skill.skillPhases.Contains(BattlePhase.Attack)) return true;
    }
    return false;
  }

  public bool CanBreakObjects() {
    return primaryWeapon.damageType == DamageType.Chop || primaryWeapon.damageType == DamageType.Crash;
  }

  public bool CanEquip(Equipment item, UnitEquipSlot slot) {
    float[] unitStats = { unit.Strength, unit.Dexterity, unit.Intelligence };

    for (int i = 0; i < item.requirementStats.Length; i++) {
      if (item.requirementStats[i] > unitStats[i]) return false;
    }
    if (item.requirementLevel > unit.Level) return false;
    if (item.slot != slot) return false;

    switch (slot) {
      case UnitEquipSlot.Primary:
        if (item is Weapon weapon1) {
          if (unit.AllowedWeapon == weapon1.type) return true;
        }
        return false;
      case UnitEquipSlot.Armor:
        if (item is Armor armor1) {
          foreach (ArmorSet set in armorSets) {
            if (set.id == armor1.id) return true;
          }
        }
        return false;
      case UnitEquipSlot.Secondary:
        if (item is Weapon weapon2) {
          // FIXME: Проверка на оружие для левой руки
        }
        else if (item is Armor armor2) {
          if (unit.ShieldIsAllow) return true;
        }
        return false;
      default:
        return false;
    }
  }
}
