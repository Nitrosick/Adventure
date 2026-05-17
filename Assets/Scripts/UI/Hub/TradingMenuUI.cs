using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TradingMenuUI : HubMenuFeature {
  public Sprite unitAvatar;
  public static TradingMenuUI Instance;

  private RectTransform resourceSlots;
  private RectTransform equipmentSlots;
  private RectTransform miscSlots;
  public int[] resourcePrices;
  private int[] resourceFinalPrices;

  private bool resourcesAvailable;
  private List<Equipment> equipmentGoods;
  private List<Item> itemGoods;

  protected override void Awake() {
    base.Awake();
    Instance = this;

    resourceSlots = transform.Find("ResourceSlots").GetComponent<RectTransform>();
    equipmentSlots = transform.Find("EquipmentSlots").GetComponent<RectTransform>();
    miscSlots = transform.Find("MiscSlots").GetComponent<RectTransform>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Trading menu UI components initialization error");
    }

    resourceFinalPrices = resourcePrices.ToArray();
  }

  private bool ComponentsInitialized() {
    return new object[] { resourceSlots, equipmentSlots, miscSlots }.All(x => x != null) && resourcePrices.Length == 4;
  }

  public void Init(
    string name,
    MasteryLevel lvl,
    bool resAvailable,
    List<Equipment> equip,
    List<Item> items,
    Sprite customAvatar = null
  ) {
    InitHeader(name, lvl);

    resourceFinalPrices = resourcePrices
      .Select(p => (int)Math.Round(p * AbilityController.PriceBonus()))
      .ToArray();

    resourcesAvailable = resAvailable;
    equipmentGoods = equip;
    itemGoods = items;
    avatar.sprite = customAvatar == null ? unitAvatar : customAvatar;

    UpdateSlotsSize(resourceSlots);
    UpdateSlotsSize(equipmentSlots);
    UpdateSlotsSize(miscSlots);
    UpdateGoodsData();
  }

  void UpdateGoodsData() {
    ClearSlots(resourceSlots);
    ClearSlots(equipmentSlots);
    ClearSlots(miscSlots);

    if (resourcesAvailable) {
      for (int i = 0; i < resourceFinalPrices.Length; i++) {
        GameObject slot = Instantiate(GameManager.I.slotWithPrice, resourceSlots);
        slot.GetComponent<SlotWithPrice>().Init(
          GameManager.I.resourceSprites[i],
          resourceFinalPrices[i],
          i,
          MapUI.Instance.resTooltips[i]
        );
      }
    }

    foreach (Equipment item in equipmentGoods) {
      GameObject slot = Instantiate(GameManager.I.slotWithPrice, equipmentSlots);
      slot.GetComponent<SlotWithPrice>().Init(item);
    }

    foreach (Item item in itemGoods) {
      GameObject slot = Instantiate(GameManager.I.slotWithPrice, miscSlots);
      slot.GetComponent<SlotWithPrice>().Init(item);
    }

    RenderEmptySlots(resourceSlots, resourcesAvailable ? resourceFinalPrices.Length : 0);
    RenderEmptySlots(equipmentSlots, equipmentGoods.Count);
    RenderEmptySlots(miscSlots, itemGoods.Count);
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(resourceSlots);
    ClearSlots(equipmentSlots);
    ClearSlots(miscSlots);
  }

  public void CheckBalance() {
    foreach (SlotWithPrice slot in FindObjectsOfType<SlotWithPrice>()) {
      slot.UpdatePrice();
    }
  }
}
