
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueueSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  private Image frame;
  private Image relationIndicator;
  private Image crown;
  private Image skull;
  private Image portrait;

  private Unit currentUnit;
  private SupportInstance currentSupport;

  private static Color activeColor;
  private static Color allyColor;
  private static Color enemyColor;

  void Awake() {
    frame = transform.Find("Frame").GetComponent<Image>();
    relationIndicator = transform.Find("RelationIndicator").GetComponent<Image>();
    crown = transform.Find("Crown").GetComponent<Image>();
    skull = transform.Find("Skull").GetComponent<Image>();
    portrait = transform.Find("Portrait").GetComponent<Image>();

    if (frame == null || relationIndicator == null || crown == null || skull == null || portrait == null) {
      Debug.LogError("Queue slot components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#EFBF0D", out activeColor);
    ColorUtility.TryParseHtmlString("#174E87", out allyColor);
    ColorUtility.TryParseHtmlString("#781010", out enemyColor);
  }

  public void Init(Unit unit) {
    currentUnit = unit;
    Color color = unit.Relation == UnitRelation.Ally ? allyColor : enemyColor;
    relationIndicator.color = color;
    crown.gameObject.SetActive(unit.IsHero);
    skull.gameObject.SetActive(unit.IsBoss);
    portrait.sprite = unit.avatar;
  }

  public void Init(SupportInstance unit) {
    currentSupport = unit;
    Color color = unit.relation == UnitRelation.Ally ? allyColor : enemyColor;
    relationIndicator.color = color;
    portrait.sprite = unit.data.icon;
  }

  public void SetActive() {
    frame.color = activeColor;
    crown.color = activeColor;
    skull.color = activeColor;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (currentUnit != null) InfoPopup.Show(currentUnit);
    else if (currentSupport != null) InfoPopup.Show(currentSupport);
  }

  public void OnPointerExit(PointerEventData eventData) {
    InfoPopup.Hide();
  }
}
