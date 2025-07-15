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
  private Transform damageMarker;
  private TextMeshProUGUI damageValue;
  private Transform defenseMarker;
  private TextMeshProUGUI defenseValue;
  private object item;
  private Action<object> callback;
  private bool disabled;

  private void Awake() {
    background = transform.GetComponent<Image>();
    image = transform.Find("Icon/Image").GetComponent<Image>();
    title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
    damageMarker = transform.Find("Damage").GetComponent<Transform>();
    damageValue = damageMarker.Find("Value").GetComponent<TextMeshProUGUI>();
    defenseMarker = transform.Find("Defense").GetComponent<Transform>();
    defenseValue = defenseMarker.Find("Value").GetComponent<TextMeshProUGUI>();

    if (
      image == null || title == null || damageMarker == null ||
      damageValue == null || defenseMarker == null || defenseValue == null ||
      background == null
    ) {
      Debug.LogError("Selector item components initialization error");
      return;
    }
  }

  public async void Init(Equipment _item, Action<object> action, bool unavailable) {
    await Task.Yield();
    item = _item;
    callback = action;
    disabled = unavailable;
    image.sprite = _item.icon;
    title.text = unavailable ? "<color=#F61010>" + _item.itemName + "</color>" : _item.itemName;

    if (_item is Weapon weapon) {
      damageMarker.gameObject.SetActive(true);
      damageValue.text = weapon.damage.ToString();
    } else if (_item is Armor armor) {
      defenseMarker.gameObject.SetActive(true);
      defenseValue.text = armor.defense.ToString();
    }
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (disabled) return;
    background.color = new Color(1, 1, 1, 0.15f);
  }

  public void OnPointerExit(PointerEventData eventData) {
    background.color = new Color(0, 0, 0, 0);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (disabled) return;
    callback(item);
    Selector.Close();
  }
}
