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
  private static GameObject unitActions;
  private static GameObject itemActions;
  private static Button unitInSquad;
  public static Button UnitDismiss { get; set; }
  private static Button useItem;
  private static Transform equipment;
  private static Button equipmentPrimary;
  private static Button equipmentArmor;
  private static Button equipmentSecondary;
  private static Button equipmentAdditional;
  private static GameObject coreStats;
  private static GameObject statPointsRow;
  private static TextMeshProUGUI statPoints;
  private static GameObject abilityPointsRow;
  private static TextMeshProUGUI abilityPoints;
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
  private static TextMeshProUGUI unitProjectiles;
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
  private static GameObject itemParams;
  private static TextMeshProUGUI itemIntensity;

  private T Find<T>(string path) where T : Component => panel.Find(path).GetComponent<T>();
  private GameObject FindGO(string path) => panel.Find(path).gameObject;

  private void Awake() {
    panel = transform.Find("PlayerMenu/Panel/Right/Viewport/Content");

    displayName = Find<TextMeshProUGUI>("Head/Data/Name");
    Level = Find<TextMeshProUGUI>("Head/Data/Level");
    type = Find<TextMeshProUGUI>("Head/Data/Type");
    inSquadMark = FindGO("Head/Data/InSquadMark");
    deathMark = FindGO("Head/Data/DeathMark");
    equippedMark = FindGO("Head/Data/EquippedMark");
    avatar = panel.Find("Head/Image");

    unitActions = FindGO("UnitActions");
    itemActions = FindGO("ItemActions");
    unitInSquad = Find<Button>("UnitActions/Activity");
    UnitDismiss = Find<Button>("UnitActions/Dismiss");
    useItem = Find<Button>("ItemActions/Use");

    equipment = panel.Find("Equipment");
    equipmentPrimary = Find<Button>("Equipment/Primary");
    equipmentArmor = Find<Button>("Equipment/Armor");
    equipmentSecondary = Find<Button>("Equipment/Secondary");
    equipmentAdditional = Find<Button>("Equipment/Additional");

    coreStats = FindGO("CoreStats");
    statPointsRow = FindGO("CoreStats/StatPoints");
    statPoints = Find<TextMeshProUGUI>("CoreStats/StatPoints/Value");
    abilityPointsRow = FindGO("CoreStats/AbilityPoints");
    abilityPoints = Find<TextMeshProUGUI>("CoreStats/AbilityPoints/Value");
    strength = Find<TextMeshProUGUI>("CoreStats/Strength/Value");
    dexterity = Find<TextMeshProUGUI>("CoreStats/Dexterity/Value");
    intelligence = Find<TextMeshProUGUI>("CoreStats/Intelligence/Value");

    strengthUp = Find<Button>("CoreStats/Strength/Plus");
    dexterityUp = Find<Button>("CoreStats/Dexterity/Plus");
    intelligenceUp = Find<Button>("CoreStats/Intelligence/Plus");

    description = Find<TextMeshProUGUI>("Description");

    unitParams = FindGO("UnitParameters");
    unitHp = Find<TextMeshProUGUI>("UnitParameters/HP/Value");
    unitMp = Find<TextMeshProUGUI>("UnitParameters/MP/Value");
    unitDamage = Find<TextMeshProUGUI>("UnitParameters/Damage/Value");
    unitDefense = Find<TextMeshProUGUI>("UnitParameters/Defense/Value");
    unitRange = Find<TextMeshProUGUI>("UnitParameters/Range/Value");
    unitProjectiles = Find<TextMeshProUGUI>("UnitParameters/Projectiles/Value");

    equipRequirements = FindGO("EquipRequirements");
    equipRequiredStats = Find<TextMeshProUGUI>("EquipRequirements/Stats/Value");
    equipRequiredLevel = Find<TextMeshProUGUI>("EquipRequirements/Level/Value");

    weaponParams = FindGO("WeaponParameters");
    weaponDamage = Find<TextMeshProUGUI>("WeaponParameters/Damage/Value");
    weaponDamageType = Find<TextMeshProUGUI>("WeaponParameters/DamageType/Value");
    weaponRange = Find<TextMeshProUGUI>("WeaponParameters/Range/Value");
    weaponCritMod = Find<TextMeshProUGUI>("WeaponParameters/CritModifier/Value");
    weaponArmorPen = Find<TextMeshProUGUI>("WeaponParameters/ArmorPen/Value");

    armorParams = FindGO("ArmorParameters");
    armorDefense = Find<TextMeshProUGUI>("ArmorParameters/Defense/Value");

    equipParams = FindGO("EquipParameters");
    equipWeight = Find<TextMeshProUGUI>("EquipParameters/Weight/Value");
    equipEffect = Find<TextMeshProUGUI>("EquipParameters/Effect/Value/Text");
    equipEffectIcon = Find<Image>("EquipParameters/Effect/Value/Icon");
    effectTip = Find<TooltipTrigger>("EquipParameters/Effect/Value");
    equipSkill = Find<TextMeshProUGUI>("EquipParameters/Skill/Value/Text");
    equipSkillIcon = Find<Image>("EquipParameters/Skill/Value/Icon");
    skillTip = Find<TooltipTrigger>("EquipParameters/Skill/Value");
    itemParams = FindGO("ItemParameters");
    itemIntensity = Find<TextMeshProUGUI>("ItemParameters/Intensity/Value");

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI components initialization error");
    }

    unitInSquad.onClick.AddListener(SwitchUnitInSquad);
    UnitDismiss.onClick.AddListener(DismissConfirmation);
    useItem.onClick.AddListener(UseItem);
    equipmentPrimary.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Primary));
    equipmentArmor.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Armor));
    equipmentSecondary.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Secondary));
    equipmentAdditional.onClick.AddListener(() => OpenSelector(UnitEquipSlot.Additional));
    strengthUp.onClick.AddListener(() => IncreaseStat(CoreStat.Strength));
    dexterityUp.onClick.AddListener(() => IncreaseStat(CoreStat.Dexterity));
    intelligenceUp.onClick.AddListener(() => IncreaseStat(CoreStat.Intelligence));
  }

  private static bool ComponentsInitialized() {
    return panel != null && displayName != null && Level != null &&
    type != null && avatar != null && unitActions != null &&
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
    deathMark != null && itemActions != null && useItem != null &&
    itemParams != null && itemIntensity != null && unitProjectiles != null &&
    equipmentAdditional != null && abilityPointsRow != null && abilityPoints != null;
  }

  private void OnDestroy() {
    unitInSquad.onClick.RemoveListener(SwitchUnitInSquad);
    UnitDismiss.onClick.RemoveListener(DismissConfirmation);
    useItem.onClick.RemoveListener(UseItem);
    equipmentPrimary.onClick.RemoveListener(() => { });
    equipmentArmor.onClick.RemoveListener(() => { });
    equipmentSecondary.onClick.RemoveListener(() => { });
    equipmentAdditional.onClick.RemoveListener(() => { });
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
    unitActions.SetActive(false);
    itemActions.SetActive(false);
    equipment.gameObject.SetActive(false);
    coreStats.SetActive(false);
    strengthUp.gameObject.SetActive(false);
    dexterityUp.gameObject.SetActive(false);
    intelligenceUp.gameObject.SetActive(false);
    statPointsRow.SetActive(false);
    abilityPointsRow.SetActive(false);
    unitParams.SetActive(false);
    equipRequirements.SetActive(false);
    weaponParams.SetActive(false);
    armorParams.SetActive(false);
    equipParams.SetActive(false);
    itemParams.SetActive(false);

    foreach (Transform child in avatar) Destroy(child.gameObject);
  }

  public static void ShowInfo(Unit unit) {
    Clear();
    Player player = Player.Instance;

    if (!unit.IsHero) unitActions.SetActive(true);
    else RecalculatePoints();

    equipment.gameObject.SetActive(true);
    coreStats.SetActive(true);
    unitParams.SetActive(true);

    PlayerMenuUI.FrameSlot();
    PlayerMenuUI.selectedSupport = null;
    PlayerMenuUI.selectedUnit = unit;

    if (unit.IsNew) {
      unit.IsNew = false;
      PlayerMenuUI.selectedSlot.HideNewMark();
    }

    displayName.text = unit.Name;
    Level.text = "Level: " + unit.Level.ToString();
    type.text = "Type: " + unit.Type.ToString();
    deathMark.SetActive(unit.CurrentHealth <= 0);
    inSquadMark.SetActive(unit.InSquad);

    GameObject unitAvatar = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    unitAvatar.GetComponent<MenuSlot>().Init(unit, true);
    InSquadButtonLabel(unit);
    UpdateUnitEquipment(unit);

    strength.text = "<color=#F61010>" + unit.Strength.ToString() + "</color>";
    dexterity.text = "<color=#81D11F>" + unit.Dexterity.ToString() + "</color>";
    intelligence.text = "<color=#2B8EF3>" + unit.Intelligence.ToString() + "</color>";
    description.text = unit.Description;

    float totalHp = unit.Health.GetMaxHP();
    float hp = unit.CurrentHealth;
    unitHp.text = string.Format(
      "{0} / {1}",
      totalHp / 3 > hp ? "<color=#F61010>" + Math.Ceiling(hp).ToString() + "</color>" : Math.Ceiling(hp).ToString(),
      totalHp.ToString()
    );

    unitMp.text = unit.DefaultMovePoints.ToString();
    unitDamage.text = (unit.Equip.primary.damage + unit.Strength).ToString();
    unitDefense.text = unit.Equip.GetTotalDefense().ToString();
    unitRange.text = unit.Equip.GetRange().ToString();

    if (unit.Projectiles == 0) unitProjectiles.text = "-";
    else if (unit.Projectiles == unit.CurrentProjectiles) unitProjectiles.text = unit.Projectiles.ToString();
    else unitProjectiles.text = $"{unit.CurrentProjectiles} / {unit.Projectiles}";
  }

  public static void ShowInfo(SupportInstance unit) {
    Clear();
    unitActions.SetActive(true);

    PlayerMenuUI.FrameSlot();
    PlayerMenuUI.selectedUnit = null;
    PlayerMenuUI.selectedSupport = unit;

    if (unit.isNew) {
      unit.isNew = false;
      PlayerMenuUI.selectedSlot.HideNewMark();
    }

    displayName.text = unit.data.unitName;
    Level.text = "Level: " + unit.level.ToString();
    type.text = "Type: " + unit.data.bonusType.ToString();
    inSquadMark.SetActive(unit.inSquad);

    GameObject unitAvatar = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    unitAvatar.GetComponent<MenuSlot>().Init(unit, true);
    InSquadButtonLabel(unit);

    description.text = unit.data.description;
    if (unit.effectDescription != "") {
      description.text += $"\n({unit.effectDescription})";
    }
  }

  public static void ShowInfo(Equipment equip) {
    Clear();
    equipRequirements.SetActive(true);
    equipParams.SetActive(true);
    PlayerMenuUI.FrameSlot();

    if (equip.isNew) {
      equip.isNew = false;
      PlayerMenuUI.selectedSlot.HideNewMark();
    }

    displayName.text = equip.itemName;
    Level.text = "Rarity: " + equip.rarity.ToString();
    type.text = "Type: " + equip.type.ToString();
    equippedMark.SetActive(PlayerMenuUI.selectedSlot.ActiveMark.activeSelf);

    GameObject icon = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    icon.GetComponent<MenuSlot>().Init(equip, true);

    int[] reqStats = equip.GetRequirementStats();
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

  public static void ShowInfo(Item item) {
    Clear();
    itemActions.SetActive(true);
    itemParams.SetActive(true);

    PlayerMenuUI.FrameSlot();
    PlayerMenuUI.selectedItem = item;

    if (item.isNew) {
      item.isNew = false;
      PlayerMenuUI.selectedSlot.HideNewMark();
    }

    displayName.text = item.itemName;
    Level.text = "Rarity: " + item.rarity.ToString();

    if (item is MedicineItem medItem) {
      type.text = "Type: Medicine item";
      itemIntensity.text = medItem.intensity + " HP";
    }
    // FIXME: Добавить все типы предметов

    GameObject icon = Instantiate(PlayerMenuUI.Instance.menuSlotPrefab, avatar);
    icon.GetComponent<MenuSlot>().Init(item, true);

    useItem.interactable = item.usable;
    description.text = item.description;
  }

  public static void InSquadButtonLabel(Unit unit) {
    unitInSquad.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = unit.InSquad
      ? "Remove from squad"
      : "Add to squad";
  }

  public static void InSquadButtonLabel(SupportInstance unit) {
    unitInSquad.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = unit.inSquad
      ? "Remove from squad"
      : "Add to squad";
  }

  public static void RecalculatePoints() {
    Player player = Player.Instance;

    if (player.StatPoints > 0) {
      statPointsRow.SetActive(true);
      strengthUp.gameObject.SetActive(true);
      dexterityUp.gameObject.SetActive(true);
      intelligenceUp.gameObject.SetActive(true);
    }
    if (player.AbilityPoints > 0) {
      abilityPointsRow.SetActive(true);
    }

    statPoints.text = player.StatPoints.ToString();
    abilityPoints.text = player.AbilityPoints.ToString();
  }

  // Outer actions
  public static void UpdateUnitEquipment(Unit unit) => PlayerMenuUIActions.UpdateUnitEquipment(unit, equipment);
  private static void SwitchUnitInSquad() => PlayerMenuUIActions.SwitchUnitInSquad(inSquadMark);
  private static void DismissConfirmation() => PlayerMenuUIActions.DismissConfirmation();
  private static void OpenSelector(UnitEquipSlot slot) => PlayerMenuUIActions.OpenSelector(slot);
  private static void IncreaseStat(CoreStat stat) => PlayerMenuUIActions.IncreaseStat(stat);
  private static void UseItem() => PlayerMenuUIActions.UseItem();
}
