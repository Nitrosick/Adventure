using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuUIInfo : MonoBehaviour {
  private static Transform panel;

  private static TextMeshProUGUI displayName;
  public static TextMeshProUGUI Level { get; set; }
  private static TextMeshProUGUI type;
  private static GameObject inSquadMark;
  private static GameObject deathMark;
  private static GameObject equippedMark;
  private static Transform avatar;
  private static GameObject actions;
  private static Button unitInSquad;
  public static Button UnitDismiss { get; set; }
  private static Transform equipment;
  private static Button equipmentPrimary;
  private static Button equipmentArmor;
  private static Button equipmentSecondary;
  private static GameObject coreStats;
  private static GameObject statPointsRow;
  private static TextMeshProUGUI statPoints;
  private static TextMeshProUGUI strength;
  private static TextMeshProUGUI dexterity;
  private static TextMeshProUGUI intelligence;
  private static Button strengthUp;
  private static Button dexterityUp;
  private static Button intelligenceUp;
  private static TextMeshProUGUI description;
  private static GameObject unitParams;
  private static TextMeshProUGUI unitHp;
  private static TextMeshProUGUI unitMp;
  private static TextMeshProUGUI unitDamage;
  private static TextMeshProUGUI unitDefense;
  private static TextMeshProUGUI unitRange;
  private static GameObject equipRequirements;
  private static TextMeshProUGUI equipRequiredStats;
  private static TextMeshProUGUI equipRequiredLevel;
  private static GameObject weaponParams;
  private static TextMeshProUGUI weaponDamage;
  private static TextMeshProUGUI weaponDamageType;
  private static TextMeshProUGUI weaponRange;
  private static TextMeshProUGUI weaponCritMod;
  private static TextMeshProUGUI weaponArmorPen;
  private static GameObject armorParams;
  private static TextMeshProUGUI armorDefense;
  private static GameObject equipParams;
  private static TextMeshProUGUI equipWeight;
  private static TextMeshProUGUI equipEffect;
  private static Image equipEffectIcon;
  private static TooltipTrigger effectTip;
  private static TextMeshProUGUI equipSkill;
  private static Image equipSkillIcon;
  private static TooltipTrigger skillTip;

  private void Awake() {
    panel = transform.Find("PlayerMenu/Panel/Right/Viewport/Content");

    displayName = panel.Find("Head/Data/Name").GetComponent<TextMeshProUGUI>();
    Level = panel.Find("Head/Data/Level").GetComponent<TextMeshProUGUI>();
    type = panel.Find("Head/Data/Type").GetComponent<TextMeshProUGUI>();
    inSquadMark = panel.Find("Head/Data/InSquadMark").gameObject;
    deathMark = panel.Find("Head/Data/DeathMark").gameObject;
    equippedMark = panel.Find("Head/Data/EquippedMark").gameObject;
    avatar = panel.Find("Head/Image").GetComponent<Transform>();
    actions = panel.Find("Actions").gameObject;
    unitInSquad = panel.Find("Actions/Activity").GetComponent<Button>();
    UnitDismiss = panel.Find("Actions/Dismiss").GetComponent<Button>();
    equipment = panel.Find("Equipment").GetComponent<Transform>();
    equipmentPrimary = panel.Find("Equipment/Primary").GetComponent<Button>();
    equipmentArmor = panel.Find("Equipment/Armor").GetComponent<Button>();
    equipmentSecondary = panel.Find("Equipment/Secondary").GetComponent<Button>();
    coreStats = panel.Find("CoreStats").gameObject;
    statPointsRow = panel.Find("CoreStats/Points").gameObject;
    statPoints = panel.Find("CoreStats/Points/Value").GetComponent<TextMeshProUGUI>();
    strength = panel.Find("CoreStats/Strength/Value").GetComponent<TextMeshProUGUI>();
    dexterity = panel.Find("CoreStats/Dexterity/Value").GetComponent<TextMeshProUGUI>();
    intelligence = panel.Find("CoreStats/Intelligence/Value").GetComponent<TextMeshProUGUI>();
    strengthUp = panel.Find("CoreStats/Strength/Plus").GetComponent<Button>();
    dexterityUp = panel.Find("CoreStats/Dexterity/Plus").GetComponent<Button>();
    intelligenceUp = panel.Find("CoreStats/Intelligence/Plus").GetComponent<Button>();
    description = panel.Find("Description").GetComponent<TextMeshProUGUI>();
    unitParams = panel.Find("UnitParameters").gameObject;
    unitHp = panel.Find("UnitParameters/HP/Value").GetComponent<TextMeshProUGUI>();
    unitMp = panel.Find("UnitParameters/MP/Value").GetComponent<TextMeshProUGUI>();
    unitDamage = panel.Find("UnitParameters/Damage/Value").GetComponent<TextMeshProUGUI>();
    unitDefense = panel.Find("UnitParameters/Defense/Value").GetComponent<TextMeshProUGUI>();
    unitRange = panel.Find("UnitParameters/Range/Value").GetComponent<TextMeshProUGUI>();
    equipRequirements = panel.Find("EquipRequirements").gameObject;
    equipRequiredStats = panel.Find("EquipRequirements/Stats/Value").GetComponent<TextMeshProUGUI>();
    equipRequiredLevel = panel.Find("EquipRequirements/Level/Value").GetComponent<TextMeshProUGUI>();
    weaponParams = panel.Find("WeaponParameters").gameObject;
    weaponDamage = panel.Find("WeaponParameters/Damage/Value").GetComponent<TextMeshProUGUI>();
    weaponDamageType = panel.Find("WeaponParameters/DamageType/Value").GetComponent<TextMeshProUGUI>();
    weaponRange = panel.Find("WeaponParameters/Range/Value").GetComponent<TextMeshProUGUI>();
    weaponCritMod = panel.Find("WeaponParameters/CritModifier/Value").GetComponent<TextMeshProUGUI>();
    weaponArmorPen = panel.Find("WeaponParameters/ArmorPen/Value").GetComponent<TextMeshProUGUI>();
    armorParams = panel.Find("ArmorParameters").gameObject;
    armorDefense = panel.Find("ArmorParameters/Defense/Value").GetComponent<TextMeshProUGUI>();
    equipParams = panel.Find("EquipParameters").gameObject;
    equipWeight = panel.Find("EquipParameters/Weight/Value").GetComponent<TextMeshProUGUI>();
    equipEffect = panel.Find("EquipParameters/Effect/Value/Text").GetComponent<TextMeshProUGUI>();
    equipEffectIcon = panel.Find("EquipParameters/Effect/Value/Icon").GetComponent<Image>();
    effectTip = panel.Find("EquipParameters/Effect/Value").GetComponent<TooltipTrigger>();
    equipSkill = panel.Find("EquipParameters/Skill/Value/Text").GetComponent<TextMeshProUGUI>();
    equipSkillIcon = panel.Find("EquipParameters/Skill/Value/Icon").GetComponent<Image>();
    skillTip = panel.Find("EquipParameters/Skill/Value").GetComponent<TooltipTrigger>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI components initialization error");
    }

    unitInSquad.onClick.AddListener(SwitchUnitInSquad);
    UnitDismiss.onClick.AddListener(DismissConfirmation);
    equipmentPrimary.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Primary));
    equipmentArmor.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Armor));
    equipmentSecondary.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Secondary));
    strengthUp.onClick.AddListener(() => IncreaseStat(CoreStat.Strength));
    dexterityUp.onClick.AddListener(() => IncreaseStat(CoreStat.Dexterity));
    intelligenceUp.onClick.AddListener(() => IncreaseStat(CoreStat.Intelligence));
  }

  private static bool ComponentsInitialized() {
    return panel != null && displayName != null && Level != null &&
    type != null && avatar != null && actions != null &&
    unitInSquad != null && UnitDismiss != null && equipment != null &&
    coreStats != null && strength != null && dexterity != null &&
    intelligence != null && description != null && unitParams != null &&
    unitMp != null && unitDamage != null && unitDefense != null &&
    unitRange != null && inSquadMark != null && equippedMark != null &&
    weaponParams != null && weaponDamage != null && weaponDamageType != null &&
    weaponRange != null && weaponCritMod != null && weaponArmorPen != null &&
    equipParams != null && equipWeight != null && equipEffect != null &&
    equipSkill != null && armorParams != null && armorDefense != null &&
    equipEffectIcon != null && equipSkillIcon != null && effectTip != null &&
    skillTip != null && equipmentPrimary != null && equipmentArmor != null &&
    equipmentSecondary != null && equipRequirements != null && equipRequiredStats != null &&
    equipRequiredLevel != null && statPoints != null && strengthUp != null &&
    dexterityUp != null && intelligenceUp != null && statPointsRow != null &&
    deathMark != null;
  }

  private void OnDestroy() {
    unitInSquad.onClick.RemoveListener(SwitchUnitInSquad);
    UnitDismiss.onClick.RemoveListener(DismissConfirmation);
    equipmentPrimary.onClick.RemoveListener(() => { });
    equipmentArmor.onClick.RemoveListener(() => { });
    equipmentSecondary.onClick.RemoveListener(() => { });
    strengthUp.onClick.RemoveListener(() => { });
    dexterityUp.onClick.RemoveListener(() => { });
    intelligenceUp.onClick.RemoveListener(() => { });
  }

  public static void Clear() {
    displayName.text = "-";
    Level.text = "Level: -";
    type.text = "Type: -";
    description.text = "";

    inSquadMark.SetActive(false);
    deathMark.SetActive(false);
    equippedMark.SetActive(false);
    actions.SetActive(false);
    equipment.gameObject.SetActive(false);
    coreStats.SetActive(false);
    strengthUp.gameObject.SetActive(false);
    dexterityUp.gameObject.SetActive(false);
    intelligenceUp.gameObject.SetActive(false);
    statPointsRow.SetActive(false);
    unitParams.SetActive(false);
    equipRequirements.SetActive(false);
    weaponParams.SetActive(false);
    armorParams.SetActive(false);
    equipParams.SetActive(false);

    foreach (Transform child in avatar) {
      Destroy(child.gameObject);
    }
  }

  public static void UpdateSlotsSize(float size) {
    foreach (RectTransform slot in equipment) {
      slot.sizeDelta = new Vector2(size, size);
    }
  }

  public static void SelectHeroTab() {
    equipment.gameObject.SetActive(true);
    coreStats.SetActive(true);

    if (Player.Instance.StatPoints > 0) {
      strengthUp.gameObject.SetActive(true);
      dexterityUp.gameObject.SetActive(true);
      intelligenceUp.gameObject.SetActive(true);
    }

    statPointsRow.SetActive(true);
    unitParams.SetActive(true);
  }

  public static void SelectUnitsTab() {
    actions.SetActive(true);
    equipment.gameObject.SetActive(true);
    coreStats.SetActive(true);
    unitParams.SetActive(true);
  }

  public static void SelectInventoryTab() {
    equipRequirements.SetActive(true);
    equipParams.SetActive(true);
  }

  public static void ShowInfo(Unit unit) {
    foreach (Transform child in avatar) Destroy(child.gameObject);
    PlayerMenuUI.FrameSlot();

    PlayerMenuUI.selectedUnit = unit;
    displayName.text = unit.Name;
    if (!unit.IsHero) Level.text = "Level: " + unit.Level.ToString();
    type.text = "Type: " + unit.Type.ToString();

    deathMark.SetActive(unit.CurrentHealth <= 0);
    inSquadMark.SetActive(unit.InSquad);

    GameObject unitAvatar = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    unitAvatar.GetComponent<MenuSlot>().Init(unit, true);

    unitInSquad.interactable = unit.CurrentHealth > 0;
    unitInSquad.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = unit.InSquad
      ? "Remove from squad"
      : "Add to squad";

    UpdateUnitEquipment(unit);

    statPoints.text = Player.Instance.StatPoints.ToString();
    strength.text = "<color=#F61010>" + unit.Strength.ToString() + "</color>";
    dexterity.text = "<color=#81D11F>" + unit.Dexterity.ToString() + "</color>";
    intelligence.text = "<color=#2B8EF3>" + unit.Intelligence.ToString() + "</color>";
    description.text = unit.Description;

    float totalHp = unit.TotalHealth;
    float hp = unit.CurrentHealth;
    unitHp.text = string.Format(
      "{0} / {1}",
      totalHp / 3 > hp ? "<color=#F61010>" + Math.Ceiling(hp).ToString() + "</color>" : Math.Ceiling(hp).ToString(),
      totalHp.ToString()
    );

    unitMp.text = unit.DefaultMovePoints.ToString();
    unitDamage.text = (unit.Equip.primaryWeapon.damage + unit.Strength).ToString();
    unitDefense.text = unit.Equip.GetTotalDefense().ToString();
    unitRange.text = unit.Equip.primaryWeapon.range.ToString();
  }

  public static void ShowInfo(Equipment equip) {
    foreach (Transform child in avatar) Destroy(child.gameObject);
    PlayerMenuUI.FrameSlot();

    displayName.text = equip.itemName;
    Level.text = "Rarity: " + equip.rarity.ToString();
    type.text = "Type: " + equip.type.ToString();
    equippedMark.SetActive(PlayerMenuUI.selectedSlot.ActiveMark.activeSelf);

    GameObject icon = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    icon.GetComponent<MenuSlot>().Init(equip, true);

    int[] reqStats = equip.requirementStats;
    equipRequiredStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      reqStats[0], reqStats[1], reqStats[2]
    );
    equipRequiredLevel.text = equip.requirementLevel.ToString();

    if (equip is Weapon weapon) {
      weaponParams.SetActive(true);
      armorParams.SetActive(false);

      weaponDamage.text = weapon.damage.ToString();
      weaponDamageType.text = weapon.damageType.ToString();
      weaponRange.text = weapon.range.ToString();
      weaponCritMod.text = "x" + weapon.critModifier;
      weaponArmorPen.text = weapon.armorPenetration + "%";
    } else if (equip is Armor armor) {
      armorParams.SetActive(true);
      weaponParams.SetActive(false);

      armorDefense.text = armor.defense.ToString();
    }

    equipWeight.text = equip.weight.ToString();
    description.text = equip.description;

    if (equip.effect != null) {
      equipEffect.text = equip.effect.effectName;
      equipEffectIcon.gameObject.SetActive(true);
      equipEffectIcon.sprite = equip.effect.uiIcon;
      equipEffectIcon.color = equip.effect.uiIconColor;
      effectTip.message = equip.effect.description;
    } else {
      equipEffect.text = "";
      equipEffectIcon.gameObject.SetActive(false);
      effectTip.message = "";
    }

    if (equip.skill != null) {
      equipSkill.text = equip.skill.displayName;
      equipSkillIcon.gameObject.SetActive(true);
      equipSkillIcon.sprite = equip.skill.uiIcon;
      equipSkillIcon.color = equip.skill.uiIconColor;
      skillTip.message = equip.skill.description;
    } else {
      equipSkill.text = "";
      equipSkillIcon.gameObject.SetActive(false);
      skillTip.message = "";
    }
  }

  private static void UpdateUnitEquipment(Unit unit) {
    UnitEquipment equip = unit.Equip;
    Image primarySlot = equipment.Find("Primary/Image").GetComponent<Image>();
    Image armorSlot = equipment.Find("Armor/Image").GetComponent<Image>();
    Image secondarySlot = equipment.Find("Secondary/Image").GetComponent<Image>();
    primarySlot.sprite = equip.primaryWeapon.icon;
    armorSlot.sprite = equip.armor.icon;

    if (equip.secondaryWeapon != null) {
      secondarySlot.enabled = true;
      secondarySlot.sprite = equip.secondaryWeapon.icon;
    }
    else if (equip.shield != null) {
      secondarySlot.enabled = true;
      secondarySlot.sprite = equip.shield.icon;
    }
    else {
      secondarySlot.enabled = false;
    }
  }

  private static void SwitchUnitInSquad() {
    Unit unit = PlayerMenuUI.selectedUnit;
    MenuSlot slot = PlayerMenuUI.selectedSlot;

    unit.InSquad = !unit.InSquad;
    inSquadMark.SetActive(unit.InSquad);
    if (slot != null) slot.SwitchActiveMark();
  }

  private static void DismissConfirmation() {
    Dialog.Confirmation(
      DismissUnit,
      "Unit dismissing",
      "Are you sure you want to dismiss this unit?\nIt will become a regular villager and lose all accumulated levels.\nIts equipment will be moved to the player's inventory."
    );
  }

  private static void DismissUnit(bool accepted) {
    if (!accepted) return;
    Player.Instance.Army.DeleteUnit(PlayerMenuUI.selectedUnit);
    PlayerMenuUI.SelectUnitsTab();
  }

  private static void OpenSelector(UnitEquipSlot slot) {
    Unit unit = PlayerMenuUI.selectedUnit;
    if (unit == null) return;
    List<Equipment> inventory = Player.Instance.Inventory.Equip;

    List<Equipment> list = new() { };
    string title = "";
    list.AddRange(inventory.Where(i => unit.Equip.CanEquip(i, slot)));

    switch (slot) {
      case UnitEquipSlot.Primary: title = "Change weapon"; break;
      case UnitEquipSlot.Armor: title = "Change armor"; break;
      case UnitEquipSlot.Secondary: title = "Change left-hand item"; break;
    }

    Selector.List(ChangeEquipment, list, title);
  }

  private static void ChangeEquipment(object item) {
    if (item is Equipment equipment) PlayerMenuUI.selectedUnit.Equip.EquipItem(equipment);
    UpdateUnitEquipment(PlayerMenuUI.selectedUnit);
  }

  private static void IncreaseStat(CoreStat stat) {
    Unit hero = Player.Instance.Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    int[] increase = { 0, 0, 0 };

    switch (stat) {
      case CoreStat.Strength: increase[0] = 1; break;
      case CoreStat.Dexterity: increase[1] = 1; break;
      case CoreStat.Intelligence: increase[2] = 1; break;
    }

    Player.Instance.SetStatPoints(-1);
    hero.IncreaseStats(increase);
    PlayerMenuUI.SelectHeroTab();
  }
}
