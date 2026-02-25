
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class SlotWithPrice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
  private Player player;
  private ItemType type;
  private Image image;
  private TextMeshProUGUI itemPriceText;

  private int itemPrice;
  private int resourceIndex;
  private string itemId;

  private Item currentItem;
  private Equipment currentEquip;

  private enum ItemType {
    Resource,
    Equipment,
    Misc
  };

  void Awake() {
    player = Player.Instance;
    image = transform.Find("Image").GetComponent<Image>();
    itemPriceText = transform.Find("Price/Value").GetComponent<TextMeshProUGUI>();

    if (player == null || image == null || itemPriceText == null) {
      Debug.LogError("Trading menu slot components initialization error");
    }
  }

  public async void Init(Sprite sprite, int price, int i, string hint = "") {
    await Task.Yield();
    type = ItemType.Resource;
    image.sprite = sprite;
    itemPrice = price;
    resourceIndex = i;
    if (hint != "") transform.GetComponent<TooltipTrigger>().message = hint;

    UpdatePrice();
  }

  public async void Init(Equipment item) {
    await Task.Yield();
    type = ItemType.Equipment;
    image.sprite = item.icon;
    itemPrice = item.GetPrice();
    itemId = item.id;
    currentEquip = item;

    UpdatePrice();
  }

  public async void Init(Item item) {
    await Task.Yield();
    type = ItemType.Misc;
    image.sprite = item.icon;
    itemPrice = item.GetPrice();
    itemId = item.id;
    currentItem = item;

    UpdatePrice();
  }

  public void UpdatePrice() {
    itemPriceText.text = itemPrice > player.Gold
      ? $"<color=#F61010>{itemPrice}</color>"
      : itemPrice.ToString();
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (currentEquip != null) InfoPopup.Show(currentEquip, true);
    else if (currentItem != null) InfoPopup.Show(currentItem, true);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (itemPrice > player.Gold) {
      _ = Toast.Show("warning", "Not enough money");
      return;
    }

    switch (type) {
      case ItemType.Resource:
        int[] temp = { 0, 0, 0, 0 };
        // TODO: Покупка стаками
        temp[resourceIndex] += 1;
        player.SetResources(temp);
        break;
      case ItemType.Equipment:
        Equipment equip = Factory.CreateEquipById(itemId);
        if (equip == null) break;
        equip.isNew = true;
        player.Inventory.AddItems(equip);
        break;
      case ItemType.Misc:
        Item item = Factory.CreateItemById(itemId);
        if (item == null) break;
        item.isNew = true;
        player.Inventory.AddItems(item);
        break;
    }

    player.SetGold(itemPrice * -1);
    _ = Toast.Show("success", "Product has been purchased");
    TradingMenuUI.Instance.CheckBalance();
  }
}
