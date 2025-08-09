using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEquipment : MonoBehaviour {
  private Unit unit;
  public Weapon primary;
  public Equipment secondary;
  public Armor armor;
  // FIXME: Доп. слот
  // public Equipment additional;

  private ArmorSet[] armorSets;
  public Transform rightHand;
  public Transform leftHand;
  private Transform beard;
  private Transform hair;

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
        if (set.showBeard && beard != null) beard.gameObject.SetActive(true);
        if (set.showHair && hair != null) hair.gameObject.SetActive(true);
      }
      else set.gameObject.SetActive(false);
    }

    foreach (Transform item in rightHand) { Destroy(item.gameObject); }
    foreach (Transform item in leftHand) { Destroy(item.gameObject); }

    if (primary != null) {
      Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + primary.name);
      if (loadedWeapon == null) return;
      GameObject weaponObj = Instantiate(loadedWeapon.prefab, rightHand);
      weaponObj.transform.SetParent(rightHand, false);
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

    primary = null;
    secondary = null;
    armor = null;

    Player.Instance.Army.UpdateState();
    Player.Instance.Inventory.UpdateState();
  }

  public List<Equipment> GetEquipmentList() {
    List<Equipment> result = new() { primary, armor };
    if (secondary != null) result.Add(secondary);
    return result;
  }

  public float GetTotalDefense() {
    float result = 0;
    if (armor != null) result += armor.defense;
    if (secondary != null) {
      if (secondary is Armor secArmor) result += secArmor.defense;
    }
    return result;
  }

  public float GetTotalDamage() {
    // FIXME: Учет предмета во второй руке
    float result = primary.damage;
    foreach (CoreStat stat in primary.scalingStats) {
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
    if (primary != null && primary.skill != null) result.Add(primary.skill);
    if (secondary != null && secondary.skill != null) result.Add(secondary.skill);
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
        // FIXME: Проверка на оружие для левой руки
        if (item is Armor armor2) {
          if (unit.ShieldIsAllow) result = 0;
        }
        break;
    }

    if (result < 0) return result;

    float[] unitStats = { unit.Strength, unit.Dexterity, unit.Intelligence };

    bool enoughStats = true;
    for (int i = 0; i < item.requirementStats.Length; i++) {
      if (item.requirementStats[i] > unitStats[i]) {
        enoughStats = false;
        break;
      }
    }
    if (item.requirementLevel <= unit.Level && enoughStats) result = 1;

    return result;
  }

  public bool HasItem(Equipment item) {
    return new Equipment[] { primary, secondary, armor }
      .Any(e => e != null && e.id == item.id);
  }
}
