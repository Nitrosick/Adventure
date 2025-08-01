using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenuUI : HomeMenuFeature {
  public Sprite weaponsmithAvatar;
  public Sprite armorerAvatar;
  public GameObject recipePrefab;

  private Image masterIcon;
  private GameObject weaponsmithDesc;
  private GameObject armorerDesc;
  private Transform recipesPanel;

  protected readonly Dictionary<Rarity, Color> rarityPalette = new();

  protected override void Awake() {
    base.Awake();

    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    masterIcon = Get<Image>("Head/Avatar/Image");
    weaponsmithDesc = transform.Find("WeaponsmithDesc").gameObject;
    armorerDesc = transform.Find("ArmorerDesc").gameObject;
    recipesPanel = Get<Transform>("Recipes");

    if (!ComponentsInitialized()) {
      Debug.LogError("Crafting menu UI components initialization error");
      return;
    }

    InitRarityPalette();
  }

  private bool ComponentsInitialized() {
    return masterIcon != null && weaponsmithDesc != null && armorerDesc != null &&
    recipesPanel != null;
  }

  private void InitRarityPalette() {
    AddColor(Rarity.Common, "#A0A0A0");
    AddColor(Rarity.Rare, "#618C2D");
    AddColor(Rarity.Epic, "#306DAB");
    AddColor(Rarity.Legendary, "#6948A4");
    AddColor(Rarity.Relic, "#CF8F0B");
  }

  private void AddColor(Rarity lvl, string hex) {
    if (ColorUtility.TryParseHtmlString(hex, out var color)) rarityPalette[lvl] = color;
  }

  public void Init(string name, MasteryLevel lvl, MapZoneFeature type, CraftingRecipe[] recipes) {
    InitHeader(name, lvl);

    switch (type) {
      case MapZoneFeature.Weaponsmith:
        masterIcon.sprite = weaponsmithAvatar;
        weaponsmithDesc.SetActive(true);
        break;
      case MapZoneFeature.Armorer:
        masterIcon.sprite = armorerAvatar;
        armorerDesc.SetActive(true);
        break;
    }

    RenderRecipes(recipes);
  }

  private void RenderRecipes(CraftingRecipe[] recipes) {
    foreach (CraftingRecipe recipe in recipes) {
      GameObject obj = Instantiate(recipePrefab, recipesPanel);
      obj.GetComponent<CraftingRecipeUI>().Init(recipe);
    }
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    weaponsmithDesc.SetActive(false);
    armorerDesc.SetActive(false);
    masterIcon.sprite = null;
    ClearSlots(recipesPanel);
  }
}
