using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerMenuUIFilters : MonoBehaviour {
  // Units
  private static GameObject unitFilters;
  private static Button allUnits;
  private static Button freeUnits;
  private static Button inSquadUnits;

  private static GameObject supportFilters;
  private static Button allSupports;
  private static Button freeSupports;
  private static Button inSquadSupports;

  // Inventory
  private static GameObject equipFilters;
  private static Button allEquip;
  private static Button weaponEquip;
  private static Button armorEquip;
  private static Button additionalEquip;

  private static GameObject itemFilters;
  private static Button allItems;
  private static Button medicineItems;
  private static Button levelingItems;
  private static Button goods;

  public static MenuFilter value;
  private readonly Dictionary<Button, UnityAction> actions = new();

  private void Awake() {
    Transform menu = transform.Find("Menu/Player");

    Transform Find(string path) => menu.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    unitFilters = Find("Left/Blocks/Left/Header/UnitFilters").gameObject;
    allUnits = Get<Button>("Left/Blocks/Left/Header/UnitFilters/All");
    freeUnits = Get<Button>("Left/Blocks/Left/Header/UnitFilters/Free");
    inSquadUnits = Get<Button>("Left/Blocks/Left/Header/UnitFilters/InSquad");

    supportFilters = Find("Left/Blocks/Right/Header/SupportFilters").gameObject;
    allSupports = Get<Button>("Left/Blocks/Right/Header/SupportFilters/All");
    freeSupports = Get<Button>("Left/Blocks/Right/Header/SupportFilters/Free");
    inSquadSupports = Get<Button>("Left/Blocks/Right/Header/SupportFilters/InSquad");

    equipFilters = Find("Left/Blocks/Left/Header/EquipFilters").gameObject;
    allEquip = Get<Button>("Left/Blocks/Left/Header/EquipFilters/All");
    weaponEquip = Get<Button>("Left/Blocks/Left/Header/EquipFilters/Weapon");
    armorEquip = Get<Button>("Left/Blocks/Left/Header/EquipFilters/Armor");
    additionalEquip = Get<Button>("Left/Blocks/Left/Header/EquipFilters/Additional");

    itemFilters = Find("Left/Blocks/Right/Header/ItemFilters").gameObject;
    allItems = Get<Button>("Left/Blocks/Right/Header/ItemFilters/All");
    medicineItems = Get<Button>("Left/Blocks/Right/Header/ItemFilters/Medicine");
    levelingItems = Get<Button>("Left/Blocks/Right/Header/ItemFilters/Leveling");
    goods = Get<Button>("Left/Blocks/Right/Header/ItemFilters/Goods");

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI filters initialization error");
      return;
    }

    actions[allUnits] = () => SetFilter(MenuFilter.AllUnits);
    actions[freeUnits] = () => SetFilter(MenuFilter.FreeUnits);
    actions[inSquadUnits] = () => SetFilter(MenuFilter.UnitsInSquad);
    actions[allSupports] = () => SetFilter(MenuFilter.AllSupports);
    actions[freeSupports] = () => SetFilter(MenuFilter.FreeSupports);
    actions[inSquadSupports] = () => SetFilter(MenuFilter.SupportsInSquad);
    actions[allEquip] = () => SetFilter(MenuFilter.AllEquipment);
    actions[weaponEquip] = () => SetFilter(MenuFilter.Weapon);
    actions[armorEquip] = () => SetFilter(MenuFilter.Armor);
    actions[additionalEquip] = () => SetFilter(MenuFilter.Additional);
    actions[allItems] = () => SetFilter(MenuFilter.AllItems);
    actions[medicineItems] = () => SetFilter(MenuFilter.Medicine);
    actions[levelingItems] = () => SetFilter(MenuFilter.Leveling);
    actions[goods] = () => SetFilter(MenuFilter.Goods);

    foreach (var pair in actions) pair.Key.onClick.AddListener(pair.Value);
  }

  private static bool ComponentsInitialized() {
    return new object[] {
      allUnits, freeUnits, inSquadUnits, unitFilters, supportFilters,
      allSupports, freeSupports, inSquadSupports, equipFilters, allEquip,
      weaponEquip, armorEquip, additionalEquip, itemFilters, allItems,
      medicineItems, levelingItems, goods
    }.All(x => x != null);
  }

  private void OnDestroy() {
    foreach (var pair in actions) pair.Key.onClick.RemoveListener(pair.Value);
  }

  public static void InitUnitFilters() {
    unitFilters.SetActive(true);
    supportFilters.SetActive(true);
  }

  public static void InitInventoryFilters() {
    equipFilters.SetActive(true);
    itemFilters.SetActive(true);
  }

  private static void SetFilter(MenuFilter _value) {
    if (value == _value) return;
    value = _value;

    switch (value) {
      case MenuFilter.AllUnits:
      case MenuFilter.FreeUnits:
      case MenuFilter.UnitsInSquad:
      case MenuFilter.AllSupports:
      case MenuFilter.FreeSupports:
      case MenuFilter.SupportsInSquad:
        PlayerMenuUI.SelectUnitsTab();
        break;
      case MenuFilter.AllEquipment:
      case MenuFilter.Weapon:
      case MenuFilter.Armor:
      case MenuFilter.Additional:
      case MenuFilter.AllItems:
      case MenuFilter.Medicine:
      case MenuFilter.Leveling:
      case MenuFilter.Goods:
        PlayerMenuUI.SelectInventoryTab();
        break;
    }
  }

  public static void Reset() {
    unitFilters.SetActive(false);
    supportFilters.SetActive(false);
    equipFilters.SetActive(false);
    itemFilters.SetActive(false);
  }
}
