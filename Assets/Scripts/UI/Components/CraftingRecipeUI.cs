using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingRecipeUI : MonoBehaviour {
  public Sprite woodSprite;
  public Sprite stoneSprite;
  public Sprite metalSprite;
  public Sprite leatherSprite;

  private TextMeshProUGUI price;
  private Transform componentsPanel;
  private SlotWithCount resultSlot;
  private Image arrowIcon;
  private Button craftButton;
  private Sprite[] sprites;
  private Color accessColor;
  private Color denyColor;
  private CraftingRecipe recipe;

  void Awake() {
    Transform panel = transform.Find("Viewport/Content").GetComponent<Transform>();
    price = panel.Find("Price/Value").GetComponent<TextMeshProUGUI>();
    componentsPanel = panel.Find("Components");
    resultSlot = panel.Find("ResultSlot").GetComponent<SlotWithCount>();
    arrowIcon = panel.Find("ArrowIcon").GetComponent<Image>();
    craftButton = panel.Find("Craft").GetComponent<Button>();
    sprites = new Sprite[] { woodSprite, stoneSprite, metalSprite, leatherSprite };

    if (
      price == null || componentsPanel == null || craftButton == null ||
      resultSlot == null || sprites.Length != 4 || arrowIcon == null
    ) {
      Debug.LogError("Crafting recipe components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#4B4A47", out accessColor);
    ColorUtility.TryParseHtmlString("#F61010", out denyColor);
    craftButton.onClick.AddListener(Craft);
  }

  void OnDestroy() {
    craftButton.onClick.RemoveListener(Craft);
  }

  public void Init(CraftingRecipe data) {
    recipe = data;
    GameObject source = Instantiate(GameManager.I.slotWithCount, componentsPanel);
    SlotWithCount slotScript = source.GetComponent<SlotWithCount>();

    if (data.sourceEquip != null) slotScript.Init(data.sourceEquip);
    if (data.resultEquip != null) resultSlot.Init(data.resultEquip, data.resultCount);
    else if (data.resultItem != null) resultSlot.Init(data.resultItem, data.resultCount);

    if (data.componentItems.Length > 0) {
      foreach (Item item in data.componentItems) {
        GameObject obj = Instantiate(GameManager.I.slotWithCount, componentsPanel);
        obj.GetComponent<SlotWithCount>().Init(item);
      }
    }

    int[] res = data.GetComponentResources();
    for (int i = 0; i < res.Length; i++) {
      if (res[i] == 0) continue;
      GameObject obj = Instantiate(GameManager.I.slotWithCount, componentsPanel);
      obj.GetComponent<SlotWithCount>().Init(sprites[i], res[i], MapUI.Instance.resTooltips[i]);
    }

    CheckEnoughResources();
  }

  public void CheckEnoughResources() {
    if (recipe == null) return;
    Player player = Player.Instance;
    bool check = true;

    if (player.Gold < recipe.cost) {
      check = false;
      price.text = $"<color=#F61010>{recipe.GetCost()}</color>";
    } else {
      price.text = recipe.GetCost().ToString();
    }

    if (recipe.sourceEquip != null) {
      if (!player.Inventory.HasItem(recipe.sourceEquip)) check = false;
    }

    foreach (Item item in recipe.componentItems) {
      if (!player.Inventory.HasItem(item)) check = false;
    }

    int[] res = recipe.GetComponentResources();
    for (int i = 0; i < res.Length; i++) {
      if (res[i] > player.Resources[i]) check = false;
    }

    craftButton.interactable = check;
    if (check) arrowIcon.color = accessColor;
    else arrowIcon.color = denyColor;
  }

  private void Craft() {
    if (recipe == null) return;
    Player player = Player.Instance;

    if (recipe.sourceEquip != null) {
      if (!player.Inventory.HasItem(recipe.sourceEquip, true)) {
        _ = Toast.Show("warning", "The required item is equipped on the unit");
        return;
      }
    }

    player.Inventory.RemoveItem(recipe.sourceEquip);
    foreach (Item item in recipe.componentItems) player.Inventory.RemoveItem(item);
    player.SetResources(recipe.GetComponentResources().Select(n => -n).ToArray());
    player.SetGold(recipe.GetCost() * -1);

    for (int i = 0; i < recipe.resultCount; i++) {
      if (recipe.resultEquip) player.Inventory.AddItems(recipe.resultEquip);
      else if (recipe.resultItem) player.Inventory.AddItems(recipe.resultItem);
    }

    _ = Toast.Show("success", "Item crafted");
    MapUI.Instance.UpdateResources();
    HubMenuUI.RecalculateRecipes();
  }
}
