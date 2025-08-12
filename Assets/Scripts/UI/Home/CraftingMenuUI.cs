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

  protected Dictionary<Rarity, Color> rarityPalette = new();

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

    rarityPalette = Utils.GetRarityPalette();
  }

  private bool ComponentsInitialized() {
    return masterIcon != null && weaponsmithDesc != null && armorerDesc != null &&
    recipesPanel != null;
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
