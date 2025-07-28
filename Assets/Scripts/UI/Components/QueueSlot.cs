
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueueSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  private GameObject frameActive;
  private Image relationIndicator;
  private GameObject crown;
  private Image portrait;

  private Unit currentUnit;

  private static Color allyColor;
  private static Color enemyColor;

  private void Awake() {
    frameActive = transform.Find("FrameActive").gameObject;
    relationIndicator = transform.Find("RelationIndicator").GetComponent<Image>();
    crown = transform.Find("Crown").gameObject;
    portrait = transform.Find("Portrait").GetComponent<Image>();

    if (frameActive == null || relationIndicator == null || crown == null || portrait == null) {
      Debug.LogError("Queue slot components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#174E87", out allyColor);
    ColorUtility.TryParseHtmlString("#781010", out enemyColor);
  }

  public void Init(Unit unit) {
    currentUnit = unit;
    Color color = unit.Relation == UnitRelation.Ally ? allyColor : enemyColor;
    relationIndicator.color = color;
    if (unit.IsHero) crown.SetActive(true);
    portrait.sprite = unit.avatar;
  }

  public void SetActive() {
    frameActive.SetActive(true);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    InfoPopup.Show(currentUnit);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
  }
}
