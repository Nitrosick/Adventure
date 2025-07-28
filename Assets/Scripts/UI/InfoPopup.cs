using System;
using TMPro;
using UnityEngine;

public class InfoPopup : MonoBehaviour {
  // Panels
  private static Transform panel;
  private static Transform unitParams;
  private static Transform equipRequirements;
  private static Transform weaponParams;
  private static Transform armorParams;
  private static Transform medicineParams;
  private static Transform pricePanel;

  // Fields
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI description;
  private static TextMeshProUGUI price;
  private static TextMeshProUGUI reqStats;
  private static TextMeshProUGUI reqLevel;
  private static TextMeshProUGUI weight;
  private static TextMeshProUGUI weaponDamage;
  private static TextMeshProUGUI weaponDamageType;
  private static TextMeshProUGUI weaponRange;
  private static TextMeshProUGUI armorDefense;
  private static TextMeshProUGUI medIntensity;
  private static TextMeshProUGUI unitHP;
  private static TextMeshProUGUI unitLevel;
  private static TextMeshProUGUI unitStats;
  private static TextMeshProUGUI unitMP;
  private static TextMeshProUGUI unitDamage;
  private static TextMeshProUGUI unitDefense;
  private static TextMeshProUGUI unitRange;
  private static TextMeshProUGUI unitSkillCharges;
  private static TextMeshProUGUI unitEffects;

  private void Awake() {
    panel = transform.Find("Info/ItemInfoPanel").GetComponent<Transform>();
    unitParams = panel.Find("UnitParams").GetComponent<Transform>();
    equipRequirements = panel.Find("EquipRequirements").GetComponent<Transform>();
    weaponParams = panel.Find("WeaponParams").GetComponent<Transform>();
    armorParams = panel.Find("ArmorParams").GetComponent<Transform>();
    medicineParams = panel.Find("MedicineParams").GetComponent<Transform>();
    pricePanel = panel.Find("Price").GetComponent<Transform>();

    title = panel.Find("Name").GetComponent<TextMeshProUGUI>();
    description = panel.Find("Description").GetComponent<TextMeshProUGUI>();
    price = pricePanel.Find("Value").GetComponent<TextMeshProUGUI>();
    reqStats = equipRequirements.Find("Stats/Value").GetComponent<TextMeshProUGUI>();
    reqLevel = equipRequirements.Find("Level/Value").GetComponent<TextMeshProUGUI>();
    weight = equipRequirements.Find("Weight/Value").GetComponent<TextMeshProUGUI>();
    weaponDamage = weaponParams.Find("Damage/Value").GetComponent<TextMeshProUGUI>();
    weaponDamageType = weaponParams.Find("DamageType/Value").GetComponent<TextMeshProUGUI>();
    weaponRange = weaponParams.Find("Range/Value").GetComponent<TextMeshProUGUI>();
    armorDefense = armorParams.Find("Defense/Value").GetComponent<TextMeshProUGUI>();
    medIntensity = medicineParams.Find("Intensity/Value").GetComponent<TextMeshProUGUI>();
    unitHP = unitParams.Find("HP/Value").GetComponent<TextMeshProUGUI>();
    unitLevel = unitParams.Find("Level/Value").GetComponent<TextMeshProUGUI>();
    unitStats = unitParams.Find("Stats/Value").GetComponent<TextMeshProUGUI>();
    unitMP = unitParams.Find("MP/Value").GetComponent<TextMeshProUGUI>();
    unitDamage = unitParams.Find("Damage/Value").GetComponent<TextMeshProUGUI>();
    unitDefense = unitParams.Find("Defense/Value").GetComponent<TextMeshProUGUI>();
    unitRange = unitParams.Find("Range/Value").GetComponent<TextMeshProUGUI>();
    unitSkillCharges = unitParams.Find("SkillCharges/Value").GetComponent<TextMeshProUGUI>();
    unitEffects = panel.Find("Effects").GetComponent<TextMeshProUGUI>();

    if (
      title == null || description == null || medicineParams == null ||
      medIntensity == null || price == null || equipRequirements == null ||
      reqStats == null || reqLevel == null || weaponParams == null ||
      weaponDamage == null || weaponDamageType == null || weaponRange == null ||
      armorParams == null || armorDefense == null || weight == null ||
      unitParams == null || unitHP == null || unitLevel == null ||
      unitStats == null || unitMP == null || unitDamage == null ||
      unitDefense == null || unitRange == null || unitSkillCharges == null ||
      unitEffects == null || pricePanel == null
    ) {
      Debug.LogError("Item info components initialization error");
      return;
    }
  }

  public static void Show(Unit unit) {
    panel.gameObject.SetActive(true);
    unitParams.gameObject.SetActive(true);
    unitEffects.gameObject.SetActive(true);

    title.text = unit.Name;
    description.text = unit.Description;
    unitHP.text = string.Format(
      "{0} / {1}",
      unit.TotalHealth / 3 > unit.CurrentHealth ? $"<color=#F61010>{Math.Ceiling(unit.CurrentHealth)}</color>" : Math.Ceiling(unit.CurrentHealth),
      unit.TotalHealth
    );
    unitLevel.text = unit.Level.ToString();
    unitStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      unit.Strength, unit.Dexterity, unit.Intelligence
    );
    unitMP.text = unit.TotalMovePoints.ToString();
    unitDamage.text = unit.Equip.primaryWeapon.damage.ToString();
    unitDefense.text = unit.Equip.GetTotalDefense().ToString();
    unitRange.text = unit.Equip.primaryWeapon.range.ToString();
    unitSkillCharges.text = unit.SkillCharges.ToString();

    string effectsText = "Effects";
    foreach (EffectInstance e in unit.Effects.ActiveEffects) {
      if (e.effectData.isNegative) effectsText += $"\n<color=#F61010>{e.effectData.effectName}</color>";
      else effectsText += $"\n<color=#81D11F>{e.effectData.effectName}</color>";
    }
    unitEffects.text = effectsText;
  }

  public static void Show(Equipment item) {
    panel.gameObject.SetActive(true);
    equipRequirements.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.price.ToString();
    int[] stats = item.requirementStats;
    reqStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      stats[0], stats[1], stats[2]
    );
    reqLevel.text = item.requirementLevel.ToString();
    weight.text = item.weight.ToString();

    if (item is Weapon weaponItem) {
      weaponParams.gameObject.SetActive(true);
      weaponDamage.text = weaponItem.damage.ToString();
      weaponDamageType.text = weaponItem.damageType.ToString();
      weaponRange.text = weaponItem.range.ToString();
    }
    else if (item is Armor armorItem) {
      armorParams.gameObject.SetActive(true);
      armorDefense.text = armorItem.defense.ToString();
    }
  }

  public static void Show(Item item) {
    panel.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.price.ToString();

    if (item is MedicineItem medItem) {
      medicineParams.gameObject.SetActive(true);
      medIntensity.text = medItem.intensity.ToString() + " HP";
    }
  }

  public static void Hide() {
    panel.gameObject.SetActive(false);
    unitParams.gameObject.SetActive(false);
    equipRequirements.gameObject.SetActive(false);
    weaponParams.gameObject.SetActive(false);
    armorParams.gameObject.SetActive(false);
    medicineParams.gameObject.SetActive(false);
    title.text = "";
    description.text = "";
    price.text = "";
    reqStats.text = "";
    reqLevel.text = "";
    weight.text = "";
    weaponDamage.text = "";
    weaponDamageType.text = "";
    weaponRange.text = "";
    armorDefense.text = "";
    medIntensity.text = "";
    unitHP.text = "";
    unitLevel.text = "";
    unitStats.text = "";
    unitMP.text = "";
    unitDamage.text = "";
    unitDefense.text = "";
    unitRange.text = "";
    unitSkillCharges.text = "";
    unitEffects.text = "";
  }
}
