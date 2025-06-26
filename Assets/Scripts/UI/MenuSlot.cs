
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSlot : MonoBehaviour, IPointerClickHandler {
  private Unit unitItem;
  private Equipment equipmentItem;
  private Image image;
  private GameObject activeFrame;
  private bool preventPointerEvents;

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    activeFrame = transform.Find("FrameActive").gameObject;

    if (activeFrame == null) {
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
    if (unit.InSquad && !preventPointerEvents) activeFrame.SetActive(true);
  }

  public void Init(Equipment equip, bool noPointer = false) {
    if (image == null) return;
    preventPointerEvents = noPointer;
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (preventPointerEvents) return;

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
