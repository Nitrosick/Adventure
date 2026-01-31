using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Selector : MonoBehaviour {
  public static Selector Instance;
  public GameObject itemPrefab;

  private static Transform window;
  private static GameObject background;
  private static GameObject placeholder;
  private static GameObject list;
  private static Button takeOff;
  private static Button cancel;
  private static TextMeshProUGUI title;
  private static Unit selectedUnit;
  private static UnitEquipSlot currentSlot;

  private void Awake() {
    Instance = this;
    window = transform.Find("Modals/Selector").GetComponent<Transform>();
    background = transform.Find("Modals/Background").gameObject;
    placeholder = window.Find("Empty").gameObject;
    list = window.Find("List").gameObject;
    takeOff = window.Find("TakeOff").GetComponent<Button>();
    cancel = window.Find("Cancel").GetComponent<Button>();
    title = window.Find("Title").GetComponent<TextMeshProUGUI>();

    if (
      window == null || background == null || cancel == null ||
      title == null || placeholder == null || list == null ||
      takeOff == null
    ) {
      Debug.LogError("Selector components initialization error");
      return;
    }

    takeOff.onClick.AddListener(TakeOff);
    cancel.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    takeOff.onClick.RemoveListener(TakeOff);
    cancel.onClick.RemoveListener(Close);
  }

  private static void Open() {
    window.gameObject.SetActive(true);
    background.SetActive(true);
  }

  public static void Close() {
    foreach (Transform child in list.transform) Destroy(child.gameObject);

    window.gameObject.SetActive(false);
    background.SetActive(false);
    placeholder.SetActive(false);
    list.SetActive(false);
    takeOff.gameObject.SetActive(false);

    title.text = "";
    selectedUnit = null;
  }

  public static void TakeOff() {
    Unit unit = PlayerMenuUI.selectedUnit;
    if (unit == null) return;
    unit.Equip.Unequip(currentSlot, true);
    PlayerMenuUIInfo.ShowInfo(unit);
    Close();
  }

  public static void List(
    Action<object> change,
    UnitEquipSlot slot,
    List<Equipment> canEquip,
    List<Equipment> cantEquip,
    string _title = ""
  ) {
    if (canEquip.Count == 0 && cantEquip.Count == 0) {
      placeholder.SetActive(true);
    } else {
      list.SetActive(true);
      foreach (Equipment item in canEquip) {
        GameObject obj = Instantiate(Instance.itemPrefab, list.transform);
        obj.GetComponent<SelectorItem>().Init(item, change, false);
      }
      foreach (Equipment item in cantEquip) {
        GameObject obj = Instantiate(Instance.itemPrefab, list.transform);
        obj.GetComponent<SelectorItem>().Init(item, change, true);
      }
    }

    selectedUnit = PlayerMenuUI.selectedUnit;

    takeOff.gameObject.SetActive(
      selectedUnit != null &&
      slot != UnitEquipSlot.Armor &&
      selectedUnit.Equip.SlotEquipped(slot)
    );

    currentSlot = slot;
    title.text = _title;
    Open();
  }
}
