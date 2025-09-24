using UnityEngine;
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

  public static MenuFilter value;

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

    if (!ComponentsInitialized()) {
      Debug.LogError("Player menu UI filters initialization error");
      return;
    }

    allUnits.onClick.AddListener(() => SetFilter(MenuFilter.AllUnits));
    freeUnits.onClick.AddListener(() => SetFilter(MenuFilter.FreeUnits));
    inSquadUnits.onClick.AddListener(() => SetFilter(MenuFilter.UnitsInSquad));

    allSupports.onClick.AddListener(() => SetFilter(MenuFilter.AllSupports));
    freeSupports.onClick.AddListener(() => SetFilter(MenuFilter.FreeSupports));
    inSquadSupports.onClick.AddListener(() => SetFilter(MenuFilter.SupportsInSquad));

    allEquip.onClick.AddListener(() => SetFilter(MenuFilter.AllEquipment));
    weaponEquip.onClick.AddListener(() => SetFilter(MenuFilter.Weapon));
    armorEquip.onClick.AddListener(() => SetFilter(MenuFilter.Armor));
    additionalEquip.onClick.AddListener(() => SetFilter(MenuFilter.Additional));

    allItems.onClick.AddListener(() => SetFilter(MenuFilter.AllItems));
    medicineItems.onClick.AddListener(() => SetFilter(MenuFilter.Medicine));
  }

  private static bool ComponentsInitialized() {
    return allUnits != null && freeUnits != null && inSquadUnits != null &&
      unitFilters != null && supportFilters != null && allSupports != null &&
      freeSupports != null && inSquadSupports != null && equipFilters != null &&
      allEquip != null && weaponEquip != null && armorEquip != null &&
      additionalEquip != null && itemFilters != null && allItems != null &&
      medicineItems != null;
  }

  private void OnDestroy() {
    allUnits.onClick.RemoveListener(() => { });
    freeUnits.onClick.RemoveListener(() => { });
    inSquadUnits.onClick.RemoveListener(() => { });

    allSupports.onClick.RemoveListener(() => { });
    freeSupports.onClick.RemoveListener(() => { });
    inSquadSupports.onClick.RemoveListener(() => { });

    allEquip.onClick.RemoveListener(() => {});
    weaponEquip.onClick.RemoveListener(() => {});
    armorEquip.onClick.RemoveListener(() => {});
    additionalEquip.onClick.RemoveListener(() => {});

    allItems.onClick.RemoveListener(() => {});
    medicineItems.onClick.RemoveListener(() => {});
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
