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

  public float GetTotalDefense() {
    float result = 0;
    if (armor != null) result += armor.defense;
    if (shield != null) result += shield.defense;
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

  public List<Equipment> GetEquipmentList() {
    List<Equipment> result = new() { primaryWeapon, armor };
    if (secondaryWeapon != null) result.Add(secondaryWeapon);
    if (shield != null) result.Add(shield);
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
}
