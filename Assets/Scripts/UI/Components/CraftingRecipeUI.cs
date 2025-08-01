using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingRecipeUI : MonoBehaviour {
  public GameObject recipeSlotPrefab;
  public Sprite woodSprite;
  public Sprite stoneSprite;
  public Sprite metalSprite;
  public Sprite leatherSprite;

  private TextMeshProUGUI price;
  private Transform componentsPanel;
  private CraftingRecipeSlot resultSlot;
  private Button craftButton;
  private Sprite[] sprites;

  private void Awake() {
    Transform panel = transform.Find("Viewport/Content").GetComponent<Transform>();
    price = panel.Find("Price/Value").GetComponent<TextMeshProUGUI>();
    componentsPanel = panel.Find("Components");
    resultSlot = panel.Find("ResultSlot").GetComponent<CraftingRecipeSlot>();
    craftButton = panel.Find("Craft").GetComponent<Button>();
    sprites = new Sprite[] { woodSprite, stoneSprite, metalSprite, leatherSprite };

    if (
      price == null || componentsPanel == null ||
      craftButton == null ||
      resultSlot == null || sprites.Length != 4
    ) {
      Debug.LogError("Crafting recipe components initialization error");
      return;
    }

    craftButton.onClick.AddListener(Craft);
  }

  private void OnDestroy() {
    craftButton.onClick.RemoveListener(Craft);
  }

  public void Init(CraftingRecipe data) {
    price.text = data.cost.ToString();
    // FIXME: Выделять красным когда мало бабла

    GameObject source = Instantiate(recipeSlotPrefab, componentsPanel);
    CraftingRecipeSlot slotScript = source.GetComponent<CraftingRecipeSlot>();
    if (data.sourceEquip != null) slotScript.Init(data.sourceEquip);
    else if (data.sourceItem != null) slotScript.Init(data.sourceItem);
    if (data.resultEquip != null) resultSlot.Init(data.resultEquip, data.resultCount);
    else if (data.resultItem != null) resultSlot.Init(data.resultItem, data.resultCount);

    if (data.componentItems.Length > 0) {
      foreach (Item item in data.componentItems) {
        GameObject obj = Instantiate(recipeSlotPrefab, componentsPanel);
        obj.GetComponent<CraftingRecipeSlot>().Init(item);
      }
    }

    int[] res = data.componentResources;
    for (int i = 0; i < res.Length; i++) {
      if (res[i] == 0) continue;
      GameObject obj = Instantiate(recipeSlotPrefab, componentsPanel);
      obj.GetComponent<CraftingRecipeSlot>().Init(sprites[i], res[i]);
    }

    // FIXME: Включить кнопку если бабла хватает
  }

  private void Craft() {

  }
}
