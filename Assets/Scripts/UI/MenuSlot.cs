
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSlot : MonoBehaviour, IPointerClickHandler {
  private Unit unitItem;
  private Equipment equipmentItem;
  private Image image;
  private GameObject activeFrame;
  private RectTransform healthBar;
  private RectTransform healthBarFill;
  private bool preventPointerEvents;

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    activeFrame = transform.Find("FrameActive").gameObject;
    healthBar = transform.Find("HealthBar").GetComponent<RectTransform>();
    healthBarFill = transform.Find("HealthBar/Fill").GetComponent<RectTransform>();

    if (activeFrame == null || healthBar == null || healthBarFill == null)  {
      Debug.LogError("Menu slot components initialization error");
    }
  }

  private void OnDestroy() {
    unitItem = null;
    equipmentItem = null;
  }

  public void Init(Unit unit, bool noPointer = false) {
    if (image == null) return;
    preventPointerEvents = noPointer;
    unitItem = unit;
    image.sprite = unitItem.avatar;

    if (!preventPointerEvents) {
      if (unit.InSquad) activeFrame.SetActive(true);
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
  }

  public void SwitchActiveFrame() {
    activeFrame.SetActive(!activeFrame.activeSelf);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (preventPointerEvents) return;
    PlayerMenuUI.selectedSlot = this;

    if (unitItem != null) {
      PlayerMenuUI.ShowInfo(unitItem);
      return;
    }

    if (equipmentItem != null) {
      PlayerMenuUI.ShowInfo(equipmentItem);
      return;
    }
  }
}
