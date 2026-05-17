using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenuUI : HubMenuFeature {
  public Sprite weaponsmithAvatar;
  public Sprite armorerAvatar;
  public GameObject recipePrefab;
  private GameObject weaponsmithDesc;
  private GameObject armorerDesc;
  private Transform recipesPanel;

  protected Dictionary<Rarity, Color> rarityPalette = new();

  protected override void Awake() {
    base.Awake();

    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

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
    return new object[] {
      weaponsmithDesc, armorerDesc, recipesPanel
    }.All(x => x != null);
  }

  public void Init(
    string name,
    MasteryLevel lvl,
    MapZoneFeature type,
    List<CraftingRecipe> recipes,
    Sprite customAvatar = null
  ) {
    InitHeader(name, lvl);

    switch (type) {
      case MapZoneFeature.Weaponsmith:
        avatar.sprite = customAvatar == null ? weaponsmithAvatar : customAvatar;
        weaponsmithDesc.SetActive(true);
        break;
      case MapZoneFeature.Armorer:
        avatar.sprite = customAvatar == null ? armorerAvatar : customAvatar;
        armorerDesc.SetActive(true);
        break;
    }

    RenderRecipes(recipes);
  }

  private void RenderRecipes(List<CraftingRecipe> recipes) {
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
    avatar.sprite = null;
    ClearSlots(recipesPanel);
  }
}
