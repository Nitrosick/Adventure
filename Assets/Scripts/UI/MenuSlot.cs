
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSlot : MonoBehaviour, IPointerClickHandler {
  public Unit UnitItem { get; private set; }
  public Equipment EquipmentItem { get; private set; }
  private Image image;
  private GameObject activeFrame;
  public GameObject ActiveMark { get; private set; }
  private RectTransform healthBar;
  private RectTransform healthBarFill;
  private bool preventPointerEvents;

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    activeFrame = transform.Find("FrameActive").gameObject;
    ActiveMark = transform.Find("ActiveMark").gameObject;
    healthBar = transform.Find("HealthBar").GetComponent<RectTransform>();
    healthBarFill = transform.Find("HealthBar/Fill").GetComponent<RectTransform>();

    if (activeFrame == null || ActiveMark == null || healthBar == null || healthBarFill == null)  {
      Debug.LogError("Menu slot components initialization error");
    }
  }

  private void OnDestroy() {
    UnitItem = null;
    EquipmentItem = null;
  }

  public void Init(Unit unit, bool noPointer = false) {
    if (image == null) return;
    preventPointerEvents = noPointer;
    UnitItem = unit;
    image.sprite = UnitItem.avatar;

    if (!preventPointerEvents) {
      if (unit.InSquad) ActiveMark.SetActive(true);
      if (unit.TotalHealth == unit.CurrentHealth) return;
      healthBar.gameObject.SetActive(true);
      float barWidth = Mathf.Abs(healthBar.rect.width);
      float percent = Mathf.Clamp01(unit.CurrentHealth / unit.TotalHealth);
      healthBarFill.sizeDelta = new Vector2(barWidth * percent, healthBarFill.sizeDelta.y);
    }
  }

  public void Init(Equipment equip, bool noPointer = false) {
    if (image == null) return;
    preventPointerEvents = noPointer;
    EquipmentItem = equip;
    image.sprite = equip.icon;
  }

  public void SwitchActiveFrame(bool on) {
    activeFrame.SetActive(on);
  }

  public void SwitchActiveMark() {
    ActiveMark.SetActive(!ActiveMark.activeSelf);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (preventPointerEvents) return;
    PlayerMenuUI.selectedSlot = this;

    if (UnitItem != null) {
      PlayerMenuUIInfo.ShowInfo(UnitItem);
      return;
    }

    if (EquipmentItem != null) {
      PlayerMenuUIInfo.ShowInfo(EquipmentItem);
      return;
    }
  }
}
