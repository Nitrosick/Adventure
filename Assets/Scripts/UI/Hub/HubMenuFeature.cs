using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubMenuFeature : MonoBehaviour {
  private TextMeshProUGUI title;
  private TextMeshProUGUI level;
  private Image avatarBackground;

  protected Dictionary<MasteryLevel, Color> palette = new();
  protected MasteryLevel masteryLevel;

  private readonly int slotColumns = 5;
  private readonly float slotsGap = 4f;

  protected virtual void Awake() {
    title = transform.Find("Head/Data/Name").GetComponent<TextMeshProUGUI>();
    level = transform.Find("Head/Data/Level").GetComponent<TextMeshProUGUI>();
    avatarBackground = transform.Find("Head/Avatar/Background").GetComponent<Image>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Trading menu UI components initialization error");
      return;
    }

    palette = Utils.GetMasteryPalette();
  }

  private bool ComponentsInitialized() {
    return new object[] {
      title, level, avatarBackground
    }.All(x => x != null);
  }

  protected void InitHeader(string name, MasteryLevel lvl) {
    masteryLevel = lvl;
    title.text = name;
    level.text = $"Level: {lvl}";
    avatarBackground.color = palette[lvl];
  }

  public virtual void Clear() {
    if (!ComponentsInitialized()) return;
    masteryLevel = MasteryLevel.Novice;
    title.text = "";
    level.text = "";
    gameObject.SetActive(false);
  }

  protected void ClearSlots(Transform slots) {
    foreach (Transform child in slots) Destroy(child.gameObject);
  }

  protected void UpdateSlotsSize(RectTransform slots) {
    GridLayoutGroup gridGroup = slots.GetComponent<GridLayoutGroup>();
    float totalWidth = slots.rect.width;
    float totalSpacing = slotsGap * (slotColumns - 1) + slotsGap * 2;
    float size = (totalWidth - totalSpacing) / slotColumns;
    gridGroup.cellSize = new Vector2(size, size);
  }

  protected void RenderEmptySlots(RectTransform panel, int filled) {
    int placeholders = filled == 0
      ? slotColumns
      : (filled % slotColumns == 0 ? 0 : slotColumns - (filled % slotColumns));

    for (int i = 0; i < placeholders; i++) Instantiate(GameManager.I.slotEmpty, panel);
  }
}
