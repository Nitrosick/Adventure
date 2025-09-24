using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuUIFilters : MonoBehaviour {
  // Units
  private static GameObject unitFilters;
  private static Button allUnits;
  private static Button freeUnits;
  private static Button inSquadUnits;

  // Inventory
  private static GameObject supportFilters;
  private static Button allSupports;
  private static Button freeSupports;
  private static Button inSquadSupports;

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
  }

  private static bool ComponentsInitialized() {
    return allUnits != null && freeUnits != null && inSquadUnits != null &&
      unitFilters != null && supportFilters != null && allSupports != null &&
      freeSupports != null && inSquadSupports != null;
  }

  private void OnDestroy() {
    allUnits.onClick.RemoveListener(() => {});
    freeUnits.onClick.RemoveListener(() => {});
    inSquadUnits.onClick.RemoveListener(() => {});
    allSupports.onClick.RemoveListener(() => {});
    freeSupports.onClick.RemoveListener(() => {});
    inSquadSupports.onClick.RemoveListener(() => {});
  }

  public static void InitUnitFilters() {
    unitFilters.SetActive(true);
    supportFilters.SetActive(true);
  }

  public static void InitInventoryFilters() {

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
    }
  }

  public static void Reset() {
    unitFilters.SetActive(false);
    supportFilters.SetActive(false);
  }
}
