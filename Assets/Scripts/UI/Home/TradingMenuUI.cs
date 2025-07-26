using UnityEngine;

public class TradingMenuUI : HomeMenuFeature {
  public static TradingMenuUI Instance;

  private RectTransform resourceSlots;
  private RectTransform equipmentSlots;
  private RectTransform miscSlots;
  public Sprite[] resourceSprites;
  public int[] resourcePrices;

  private bool resourcesAvailable;
  private Equipment[] equipmentGoods;
  private Item[] itemGoods;

  protected override void Awake() {
    base.Awake();
    Instance = this;

    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    resourceSlots = Get<RectTransform>("ResourceSlots");
    equipmentSlots = Get<RectTransform>("EquipmentSlots");
    miscSlots = Get<RectTransform>("MiscSlots");

    if (!ComponentsInitialized()) {
      Debug.LogError("Trading menu UI components initialization error");
    }
  }

  private bool ComponentsInitialized() {
    return resourceSprites.Length == 4 && resourcePrices.Length == 4 && resourceSlots != null &&
    equipmentSlots != null && miscSlots != null;
  }

  public void Init(
    string name,
    MasteryLevel lvl,
    bool resAvailable,
    Equipment[] equip,
    Item[] items
  ) {
    InitHeader(name, lvl);

    resourcesAvailable = resAvailable;
    equipmentGoods = equip;
    itemGoods = items;

    UpdateSlotsSize(resourceSlots);
    UpdateSlotsSize(equipmentSlots);
    UpdateSlotsSize(miscSlots);
    UpdateGoodsData();
  }

  private void UpdateGoodsData() {
    ClearSlots(resourceSlots);
    ClearSlots(equipmentSlots);
    ClearSlots(miscSlots);

    if (resourcesAvailable) {
      for (int i = 0; i < resourcePrices.Length; i++) {
        GameObject slot = Instantiate(slotPrefab, resourceSlots);
        slot.GetComponent<TradingMenuSlot>().Init(resourceSprites[i], resourcePrices[i], i);
      }
    }

    foreach (Equipment item in equipmentGoods) {
      GameObject slot = Instantiate(slotPrefab, equipmentSlots);
      slot.GetComponent<TradingMenuSlot>().Init(item);
    }

    foreach (Item item in itemGoods) {
      GameObject slot = Instantiate(slotPrefab, miscSlots);
      slot.GetComponent<TradingMenuSlot>().Init(item);
    }

    RenderEmptySlots(resourceSlots, resourcesAvailable ? resourcePrices.Length : 0);
    RenderEmptySlots(equipmentSlots, equipmentGoods.Length);
    RenderEmptySlots(miscSlots, itemGoods.Length);
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(resourceSlots);
    ClearSlots(equipmentSlots);
    ClearSlots(miscSlots);
  }

  public void CheckBalance() {
    foreach (TradingMenuSlot slot in FindObjectsOfType<TradingMenuSlot>()) {
      slot.UpdatePrice();
    }
  }
}
