using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenuFeature : MonoBehaviour {
  public GameObject slotPrefab;
  public GameObject slotEmptyPrefab;

  private TextMeshProUGUI title;
  private TextMeshProUGUI level;
  private Image avatarBackground;

  protected readonly Dictionary<MasteryLevel, Color> palette = new();
  protected MasteryLevel masteryLevel;

  private readonly int slotColumns = 5;
  private readonly float slotsGap = 4f;

  protected virtual void Awake() {
    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    title = Get<TextMeshProUGUI>("Head/Data/Name");
    level = Get<TextMeshProUGUI>("Head/Data/Level");
    avatarBackground = Get<Image>("Head/Avatar/Background");

    if (!ComponentsInitialized()) {
      Debug.LogError("Trading menu UI components initialization error");
      return;
    }

    InitPalette();
  }

  private bool ComponentsInitialized() {
    return title != null && level != null && avatarBackground != null;
  }

  private void InitPalette() {
    AddColor(MasteryLevel.Novice, "#A0A0A0");
    AddColor(MasteryLevel.Apprentice, "#618C2D");
    AddColor(MasteryLevel.Adept, "#306DAB");
    AddColor(MasteryLevel.Expert, "#6948A4");
    AddColor(MasteryLevel.Master, "#CF8F0B");
  }

  private void AddColor(MasteryLevel lvl, string hex) {
    if (ColorUtility.TryParseHtmlString(hex, out var color)) palette[lvl] = color;
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

    for (int i = 0; i < placeholders; i++) Instantiate(slotEmptyPrefab, panel);
  }
}
