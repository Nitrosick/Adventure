using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SelectorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
  private Image background;
  private Image image;
  private TextMeshProUGUI title;
  private object item;
  private Action<object> callback;
  private bool disabled;

  private void Awake() {
    background = transform.GetComponent<Image>();
    image = transform.Find("Icon/Image").GetComponent<Image>();
    title = transform.Find("Title").GetComponent<TextMeshProUGUI>();

    if (image == null || title == null || background == null) {
      Debug.LogError("Selector item components initialization error");
    }
  }

  public async void Init(Equipment _item, Action<object> action, bool unavailable) {
    await Task.Yield();
    item = _item;
    callback = action;
    disabled = unavailable;
    image.sprite = _item.icon;
    title.text = unavailable ? "<color=#F61010>" + _item.itemName + "</color>" : _item.itemName;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (item is Equipment equip) InfoPopup.Show(equip);
    if (disabled) return;
    background.color = new Color(1, 1, 1, 0.15f);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
    background.color = new Color(0, 0, 0, 0);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (disabled) return;
    InfoPopup.Hide();
    callback(item);
    Selector.Close();
  }
}
