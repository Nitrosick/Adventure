using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingRecipeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  private Image image;
  private GameObject itemCount;
  private TextMeshProUGUI itemCountText;

  private Item currentItem;
  private Equipment currentEquip;

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    itemCount = transform.Find("Count").gameObject;
    itemCountText = transform.Find("Count/Value").GetComponent<TextMeshProUGUI>();

    if (image == null || itemCount == null || itemCountText == null) {
      Debug.LogError("Crafting recipe slot components initialization error");
    }
  }

  public void Init(Sprite sprite, int count = 1) {
    image.sprite = sprite;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
  }

  public void Init(Equipment item, int count = 1) {
    image.sprite = item.icon;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
    currentEquip = item;
  }

  public void Init(Item item, int count = 1) {
    image.sprite = item.icon;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
    currentItem = item;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (currentEquip != null) InfoPopup.Show(currentEquip);
    else if (currentItem != null) InfoPopup.Show(currentItem);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
  }
}
