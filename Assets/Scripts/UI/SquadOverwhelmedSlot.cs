
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class SquadOverwhelmedSlot : MonoBehaviour, IPointerClickHandler {
  public Unit UnitItem { get; private set; }
  private Image image;
  public GameObject ActiveMark { get; private set; }
  public GameObject DeathMark { get; private set; }

  private void Awake() {
    image = transform.Find("Image").GetComponent<Image>();
    ActiveMark = transform.Find("ActiveMark").gameObject;
    DeathMark = transform.Find("Dead").gameObject;

    if (ActiveMark == null || DeathMark == null) {
      Debug.LogError("Squad overwhelmed slot components initialization error");
    }
  }

  private void OnDestroy() {
    UnitItem = null;
  }

  public async void Init(Unit unit) {
    await Task.Yield();
    if (image == null) return;
    UnitItem = unit;
    image.sprite = UnitItem.avatar;

    if (UnitItem.InSquad) ActiveMark.SetActive(true);
    if (UnitItem.CurrentHealth <= 0) DeathMark.SetActive(true);
  }

  public void SwitchActivity() {
    ActiveMark.SetActive(!ActiveMark.activeSelf);
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (UnitItem == null || UnitItem.IsHero || UnitItem.CurrentHealth <= 0) return;
    UnitItem.InSquad = !UnitItem.InSquad;
    SquadOverwhelmed.Recalculate();
    SwitchActivity();
  }
}
