using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadOverwhelmed : MonoBehaviour {
  public static SquadOverwhelmed Instance;
  private static Transform window;
  private static GameObject background;
  private static TextMeshProUGUI text;
  private static Transform slots;
  private static Button confirm;
  private static Button cancel;

  public GameObject slotPrefab;
  public GameObject emptySlotPrefab;
  private static int limit;
  private static MapZoneEvent mapZoneEvent;

  private static readonly int slotsInRow = 5;

  private void Awake() {
    Instance = this;
    window = transform.Find("SquadOverwhelmed/Panel").GetComponent<Transform>();
    background = transform.Find("SquadOverwhelmed/Background").gameObject;
    text = window.Find("Text").GetComponent<TextMeshProUGUI>();
    slots = window.Find("Slots/Viewport/Content");
    confirm = window.Find("Control/Confirm").GetComponent<Button>();
    cancel = window.Find("Control/Cancel").GetComponent<Button>();

    if (
      window == null || background == null || text == null ||
      confirm == null || slots == null || cancel == null
    ) {
      Debug.LogError("Squad overwhelmed components initialization error");
      return;
    }

    confirm.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(OnCancel);
  }

  private void OnDestroy() {
    confirm.onClick.RemoveListener(OnSubmit);
    cancel.onClick.RemoveListener(OnCancel);
  }

  public static void Open(int lim, MapZoneEvent evt, bool cancelable) {
    cancel.gameObject.SetActive(cancelable);
    limit = lim;
    mapZoneEvent = evt;

    text.text =
      "Your squad is overwhelmed.\nA maximum of <color=#781010>" +
      limit.ToString() +
      "</color> units can participate in battle.\nKeep only the most important ones.";

    List<Unit> units = Player.Instance.Army.Units;

    foreach (Unit unit in units) {
      GameObject slot = Instantiate(Instance.slotPrefab, slots);
      slot.GetComponent<SquadOverwhelmedSlot>().Init(unit);
    }

    RenderEmptySlots(units.Count);
    Recalculate();
    window.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.ShowBackground();
  }

  private static void Close() {
    window.gameObject.SetActive(false);
    background.SetActive(false);
    SceneController.HideBackground();
    limit = 0;
    text.text = "";

    foreach (Transform child in slots) {
      Destroy(child.gameObject);
    }
  }

  private static void OnSubmit() {
    Close();
    if (mapZoneEvent != null) mapZoneEvent.CheckEvents();
    mapZoneEvent = null;
  }

  private static void OnCancel() {
    Close();
    mapZoneEvent = null;
  }

  private static void RenderEmptySlots(int filled) {
    double defaultSlotsCount = Math.Pow(slotsInRow, 2);

    if (filled == defaultSlotsCount) {
      return;
    } else if (filled < defaultSlotsCount) {
      for (int i = filled; i < defaultSlotsCount; i++) {
        Instantiate(Instance.emptySlotPrefab, slots);
      }
    } else {
      int remainder = filled % slotsInRow;
      int placeholders = remainder == 0 ? 0 : slotsInRow - remainder;

      for (int i = 0; i < placeholders; i++) {
        Instantiate(Instance.emptySlotPrefab, slots);
      }
    }
  }

  public static void Recalculate() {
    int inSquad = Player.Instance.Army.Units
      .Where(u => u.InSquad)
      .ToArray()
      .Length;

    confirm.interactable = inSquad <= limit;
  }
}
