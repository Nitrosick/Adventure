using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotWithCount : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  private Image image;
  private Image background;
  private GameObject itemCount;
  private TextMeshProUGUI itemCountText;

  private Item currentItem;
  private Equipment currentEquip;
  private Unit currentUnit;
  private SupportInstance currentSupport;

  void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    background = transform.Find("Background").GetComponent<Image>();
    itemCount = transform.Find("Count").gameObject;
    itemCountText = transform.Find("Count/Value").GetComponent<TextMeshProUGUI>();

    if (image == null || background == null || itemCount == null || itemCountText == null) {
      Debug.LogError("Crafting recipe slot components initialization error");
    }
  }

  public async void Init(Sprite sprite, int count = 1, string hint = "") {
    await Task.Yield();
    image.sprite = sprite;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
    if (hint != "") transform.GetComponent<TooltipTrigger>().message = hint;
  }

  public async void Init(Equipment item, int count = 1) {
    await Task.Yield();
    image.sprite = item.icon;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
    currentEquip = item;
  }

  public async void Init(Item item, int count = 1) {
    await Task.Yield();
    image.sprite = item.icon;
    if (count > 1) itemCount.SetActive(true);
    itemCountText.text = count.ToString();
    currentItem = item;
  }

  public async void Init(Unit unit) {
    await Task.Yield();
    image.sprite = unit.avatar;
    currentUnit = unit;
  }

  public async void Init(SupportInstance unit) {
    await Task.Yield();
    image.sprite = unit.data.icon;
    background.color = MapUI.Instance.palette[unit.level];
    currentSupport = unit;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (currentEquip != null) InfoPopup.Show(currentEquip);
    else if (currentItem != null) InfoPopup.Show(currentItem);
    else if (currentUnit != null) InfoPopup.Show(currentUnit);
    else if (currentSupport != null) InfoPopup.Show(currentSupport);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
  }
}
