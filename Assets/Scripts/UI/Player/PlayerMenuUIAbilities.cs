using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuUIAbilities : MonoBehaviour {
  public static PlayerMenuUIAbilities Instance;
  public GameObject abilityButton;
  private static RectTransform panel;
  public static Dictionary<AbilityLevel, Color> palette = new();

  private static readonly int slotColumns = 6;
  private static readonly float slotsGap = 12f;
  private static readonly float scrollWidth = 15f;

  void Awake() {
    Instance = this;
    Transform Find(string path) => transform.Find(path);
    panel = Find("Viewport/Content").GetComponent<RectTransform>();

    if (panel == null) {
      Debug.LogError("Abilities UI components initialization error");
      return;
    }

    palette = Utils.GetAbilityLevelPalette();
  }

  public static void Init() {
    Clear();

    foreach (AbilityInstance ability in AbilityController.allAbilities) {
      GameObject abilityObj = Instantiate(Instance.abilityButton, panel);
      abilityObj.GetComponent<AbilitySlot>().Init(ability);
    }

    UpdateSlotsSize();
  }

  public static void Clear() {
    foreach (Transform child in panel) Destroy(child.gameObject);
  }

  private static void UpdateSlotsSize() {
    GridLayoutGroup gridGroup = panel.GetComponent<GridLayoutGroup>();
    float totalWidth = panel.rect.width - scrollWidth * 2;
    float totalSpacing = slotsGap * (slotColumns - 1) + slotsGap * 2;
    float slotSize = (totalWidth - totalSpacing) / slotColumns;
    gridGroup.cellSize = new Vector2(slotSize, slotSize);
  }
}
