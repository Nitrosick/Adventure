
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuSlot : MonoBehaviour, IPointerClickHandler {
  public Unit UnitItem { get; private set; }
  public Equipment EquipmentItem { get; private set; }
  public Item InventoryItem { get; private set; }

  private Image image;
  private GameObject activeFrame;
  public GameObject NewMark { get; private set; }
  public GameObject ActiveMark { get; private set; }
  public GameObject DeathMark { get; private set; }
  private RectTransform healthBar;
  private RectTransform healthBarFill;
  private TextMeshProUGUI count;
  private bool preventPointerEvents;

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    activeFrame = transform.Find("FrameActive").gameObject;
    NewMark = transform.Find("NewMark").gameObject;
    ActiveMark = transform.Find("ActiveMark").gameObject;
    DeathMark = transform.Find("Dead").gameObject;
    healthBar = transform.Find("HealthBar").GetComponent<RectTransform>();
    healthBarFill = transform.Find("HealthBar/Fill").GetComponent<RectTransform>();
    count = transform.Find("Count").GetComponent<TextMeshProUGUI>();

    if (
      activeFrame == null || ActiveMark == null || DeathMark == null ||
      healthBar == null || healthBarFill == null || count == null ||
      image == null || NewMark == null
    )  {
      Debug.LogError("Menu slot components initialization error");
    }
  }

  private void OnDestroy() {
    UnitItem = null;
    EquipmentItem = null;
    InventoryItem = null;
  }

  public void Init(Unit unit, bool noPointer = false) {
    preventPointerEvents = noPointer;
    UnitItem = unit;
    image.sprite = UnitItem.avatar;
    if (unit.IsNew) NewMark.SetActive(true);

    if (!preventPointerEvents) {
      if (unit.InSquad) ActiveMark.SetActive(true);
      if (unit.TotalHealth == unit.CurrentHealth) return;
      if (unit.CurrentHealth <= 0) DeathMark.SetActive(true);
      else {
        healthBar.gameObject.SetActive(true);
        float barWidth = Mathf.Abs(healthBar.rect.width);
        float percent = Mathf.Clamp01(unit.CurrentHealth / unit.TotalHealth);
        healthBarFill.sizeDelta = new Vector2(barWidth * percent, healthBarFill.sizeDelta.y);
      }
    }
  }

  public void Init(Equipment equip, bool noPointer = false, int cnt = 1) {
    preventPointerEvents = noPointer;
    EquipmentItem = equip;
    image.sprite = equip.icon;
    if (equip.isNew) NewMark.SetActive(true);
    if (cnt > 1) {
      count.gameObject.SetActive(true);
      count.text = cnt.ToString();
    }
  }

  public void Init(Item item, bool noPointer = false, int cnt = 1) {
    preventPointerEvents = noPointer;
    InventoryItem = item;
    image.sprite = item.icon;
    if (item.isNew) NewMark.SetActive(true);
    if (cnt > 1) {
      count.gameObject.SetActive(true);
      count.text = cnt.ToString();
    }
  }

  public void SwitchActiveFrame(bool on) {
    activeFrame.SetActive(on);
  }

  public void HideNewMark() {
    NewMark.SetActive(false);
  }

  public void SwitchActiveMark() {
    ActiveMark.SetActive(!ActiveMark.activeSelf);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (preventPointerEvents) return;
    PlayerMenuUI.selectedSlot = this;

    if (UnitItem != null) PlayerMenuUIInfo.ShowInfo(UnitItem);
    else if (EquipmentItem != null) PlayerMenuUIInfo.ShowInfo(EquipmentItem);
    else if (InventoryItem != null) PlayerMenuUIInfo.ShowInfo(InventoryItem);
  }
}
