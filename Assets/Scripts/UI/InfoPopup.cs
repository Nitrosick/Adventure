using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InfoPopup : MonoBehaviour {
  // Panels
  private static Transform panel;
  private static Transform unitParams;
  private static Transform supportParams;
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
  private static TextMeshProUGUI unitProjectiles;
  private static GameObject unitEffectsTitle;
  private static TextMeshProUGUI unitEffects;
  private static TextMeshProUGUI supportLevel;

  Transform Get(string path) => transform.Find(path);
  Transform Find(Transform root, string path) => root.Find(path);
  T Get<T>(Transform root, string path) where T : Component => root.Find(path).GetComponent<T>();

  void Awake() {
    panel = Get("Info/ItemInfoPanel");

    unitParams = Find(panel, "UnitParams");
    supportParams = Find(panel, "SupportParams");
    equipRequirements = Find(panel, "EquipRequirements");
    weaponParams = Find(panel, "WeaponParams");
    armorParams = Find(panel, "ArmorParams");
    medicineParams = Find(panel, "MedicineParams");
    pricePanel = Find(panel, "Price");

    title = Get<TextMeshProUGUI>(panel, "Name");
    description = Get<TextMeshProUGUI>(panel, "Description");
    price = Get<TextMeshProUGUI>(pricePanel, "Value");

    reqStats = Get<TextMeshProUGUI>(equipRequirements, "Stats/Value");
    reqLevel = Get<TextMeshProUGUI>(equipRequirements, "Level/Value");
    weight = Get<TextMeshProUGUI>(equipRequirements, "Weight/Value");

    weaponDamage = Get<TextMeshProUGUI>(weaponParams, "Damage/Value");
    weaponDamageType = Get<TextMeshProUGUI>(weaponParams, "DamageType/Value");
    weaponRange = Get<TextMeshProUGUI>(weaponParams, "Range/Value");

    armorDefense = Get<TextMeshProUGUI>(armorParams, "Defense/Value");

    medIntensity = Get<TextMeshProUGUI>(medicineParams, "Intensity/Value");

    unitHP = Get<TextMeshProUGUI>(unitParams, "HP/Value");
    unitLevel = Get<TextMeshProUGUI>(unitParams, "Level/Value");
    unitStats = Get<TextMeshProUGUI>(unitParams, "Stats/Value");
    unitMP = Get<TextMeshProUGUI>(unitParams, "MP/Value");
    unitDamage = Get<TextMeshProUGUI>(unitParams, "Damage/Value");
    unitDefense = Get<TextMeshProUGUI>(unitParams, "Defense/Value");
    unitRange = Get<TextMeshProUGUI>(unitParams, "Range/Value");
    unitProjectiles = Get<TextMeshProUGUI>(unitParams, "Projectiles/Value");

    unitEffectsTitle = Find(panel, "EffectsTitle").gameObject;
    unitEffects = Get<TextMeshProUGUI>(panel, "Effects");

    supportLevel = Get<TextMeshProUGUI>(supportParams, "Level/Value");

    if (
      title == null || description == null || medicineParams == null ||
      medIntensity == null || price == null || equipRequirements == null ||
      reqStats == null || reqLevel == null || weaponParams == null ||
      weaponDamage == null || weaponDamageType == null || weaponRange == null ||
      armorParams == null || armorDefense == null || weight == null ||
      unitParams == null || unitHP == null || unitLevel == null ||
      unitStats == null || unitMP == null || unitDamage == null ||
      unitDefense == null || unitRange == null || unitEffects == null ||
      pricePanel == null || unitProjectiles == null || unitEffectsTitle == null ||
      supportParams == null || supportLevel == null
    ) {
      Debug.LogError("Item info components initialization error");
      return;
    }
  }

  public static void Show(Unit unit, bool showPrice = false) {
    panel.gameObject.SetActive(true);
    unitParams.gameObject.SetActive(true);
    unitEffectsTitle.SetActive(true);
    unitEffects.gameObject.SetActive(true);
    if (showPrice) pricePanel.gameObject.SetActive(true);

    title.text = unit.Name;
    description.text = unit.Description;
    unitHP.text = string.Format(
      "{0} / {1}",
      unit.Health.GetMaxHP() / 3 > unit.CurrentHealth ? $"<color=#F61010>{Math.Ceiling(unit.CurrentHealth)}</color>" : Math.Ceiling(unit.CurrentHealth),
      unit.Health.GetMaxHP()
    );
    unitLevel.text = unit.Level.ToString();
    unitStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      unit.Strength, unit.Dexterity, unit.Intelligence
    );
    unitMP.text = unit.TotalMovePoints.ToString();
    unitDamage.text = unit.Equip.GetTotalDamage().ToString();
    unitDefense.text = unit.Equip.GetTotalDefense().ToString();
    unitRange.text = Math.Floor(unit.Equip.GetRange()).ToString();

    if (unit.Projectiles == 0) unitProjectiles.text = "-";
    else if (unit.Projectiles == unit.CurrentProjectiles) unitProjectiles.text = unit.Projectiles.ToString();
    else unitProjectiles.text = $"{unit.CurrentProjectiles} / {unit.Projectiles}";

    UnitEffects effectsComponent = unit.Effects;
    if (effectsComponent == null) {
      unitEffectsTitle.SetActive(false);
      unitEffects.gameObject.SetActive(false);
      return;
    }

    List<string> effectsEl = new();
    foreach (EffectInstance e in unit.Effects.ActiveEffects) {
      if (e.effectData.isNegative) effectsEl.Add($"<color=#F61010>{e.effectData.effectName}</color>");
      else effectsEl.Add($"<color=#81D11F>{e.effectData.effectName}</color>");
    }
    unitEffects.text = string.Join("\n", effectsEl);
  }

  public static void Show(Equipment item, bool showPrice = false) {
    panel.gameObject.SetActive(true);
    equipRequirements.gameObject.SetActive(true);
    if (showPrice) pricePanel.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.GetPrice().ToString();
    int[] stats = item.GetRequirementStats();
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
      weaponRange.text = Math.Floor(weaponItem.range).ToString();
    }
    else if (item is Armor armorItem) {
      armorParams.gameObject.SetActive(true);
      armorDefense.text = armorItem.defense.ToString();
    }
  }

  public static void Show(Item item, bool showPrice = false) {
    panel.gameObject.SetActive(true);
    if (showPrice) pricePanel.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.GetPrice().ToString();

    if (item is MedicineItem medItem) {
      medicineParams.gameObject.SetActive(true);
      medIntensity.text = medItem.intensity.ToString() + " HP";
    }
  }

  public static void Show(AdditionalItem item, bool showPrice = false) {
    panel.gameObject.SetActive(true);
    equipRequirements.gameObject.SetActive(true);
    if (showPrice) pricePanel.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.GetPrice().ToString();
    int[] stats = item.GetRequirementStats();
    reqStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      stats[0], stats[1], stats[2]
    );
    reqLevel.text = item.requirementLevel.ToString();
    weight.text = item.weight.ToString();
  }

  public static void Show(SupportInstance unit) {
    panel.gameObject.SetActive(true);
    supportParams.gameObject.SetActive(true);

    title.text = unit.data.unitName;
    supportLevel.text = unit.level.ToString();
    description.text = unit.data.description;

    if (unit.effectDescription != "") {
      description.text += $"\n({unit.effectDescription})";
    }
  }

  public static void Hide() {
    panel.gameObject.SetActive(false);
    unitParams.gameObject.SetActive(false);
    supportParams.gameObject.SetActive(false);
    equipRequirements.gameObject.SetActive(false);
    weaponParams.gameObject.SetActive(false);
    armorParams.gameObject.SetActive(false);
    medicineParams.gameObject.SetActive(false);
    pricePanel.gameObject.SetActive(false);
    unitEffectsTitle.SetActive(false);

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
    unitProjectiles.text = "";
    unitEffects.text = "";
    supportLevel.text = "";
  }
}
