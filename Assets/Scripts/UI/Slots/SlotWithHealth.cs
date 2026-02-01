
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SlotWithHealth : MonoBehaviour {
  private Image image;
  private GameObject deathMark;
  private RectTransform healthBar;
  private RectTransform healthBarFill;

  void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    deathMark = transform.Find("Dead").gameObject;
    healthBar = transform.Find("HealthBar").GetComponent<RectTransform>();
    healthBarFill = transform.Find("HealthBar/Fill").GetComponent<RectTransform>();

    if (image == null || deathMark == null || healthBar == null || healthBarFill == null)  {
      Debug.LogError("Healing menu slot components initialization error");
    }
  }

  public async void Init(Unit unit, bool withHp = false) {
    await Task.Yield();
    image.sprite = unit.avatar;

    if (unit.CurrentHealth <= 0) deathMark.SetActive(true);
    else if (withHp) {
      healthBar.gameObject.SetActive(true);
      float barWidth = Mathf.Abs(healthBar.rect.width);
      float percent = Mathf.Clamp01(unit.CurrentHealth / unit.Health.GetMaxHP());
      healthBarFill.sizeDelta = new Vector2(barWidth * percent, healthBarFill.sizeDelta.y);
    }
  }
}
