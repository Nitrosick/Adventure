using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingChainUI : MonoBehaviour {
  private TextMeshProUGUI price;
  private Transform componentsPanel;
  private SlotWithCount resultSlot;
  private Image arrowIcon;
  private Button trainButton;
  private Color accessColor;
  private Color denyColor;
  private TrainingChain chain;
  private readonly List<Unit> unitObjects = new() { };

  void Awake() {
    Transform panel = transform.Find("Viewport/Content").GetComponent<Transform>();
    price = panel.Find("Price/Value").GetComponent<TextMeshProUGUI>();
    componentsPanel = panel.Find("Components");
    resultSlot = panel.Find("ResultSlot").GetComponent<SlotWithCount>();
    arrowIcon = panel.Find("ArrowIcon").GetComponent<Image>();
    trainButton = panel.Find("Train").GetComponent<Button>();

    if (
      price == null || componentsPanel == null || trainButton == null ||
      resultSlot == null || arrowIcon == null
    ) {
      Debug.LogError("Training chain components initialization error");
      return;
    }

    ColorUtility.TryParseHtmlString("#4B4A47", out accessColor);
    ColorUtility.TryParseHtmlString("#F61010", out denyColor);
    trainButton.onClick.AddListener(Train);
  }

  void OnDestroy() {
    trainButton.onClick.RemoveListener(Train);
    foreach (Unit unit in unitObjects) Destroy(unit.gameObject);
    unitObjects.Clear();
  }

  public void Init(TrainingChain data) {
    chain = data;
    GameObject source = Instantiate(GameManager.I.slotWithCount, componentsPanel);
    SlotWithCount slotScript = source.GetComponent<SlotWithCount>();

    if (data.sourceUnit != null) {
      Unit prefab = StateManager.PrefabDatabase.GetPrefab(data.sourceUnit.PrefabId, true);
      unitObjects.Add(prefab);
      slotScript.Init(prefab);
    } else if (data.sourceSupport != null) {
      SupportInstance support = new (data.sourceSupport, data.sourceSupportLevel);
      slotScript.Init(support);
    } else if (data.sourceVillagersCount > 0) {
      slotScript.Init(GameManager.I.villagersSprite, data.sourceVillagersCount, "Villagers");
    }

    if (data.resultUnit != null) {
      Unit prefab = StateManager.PrefabDatabase.GetPrefab(data.resultUnit.PrefabId, true);
      unitObjects.Add(prefab);
      resultSlot.Init(prefab);
    } else if (data.resultSupport != null) {
      SupportInstance support = new (data.resultSupport, data.resultSupportLevel);
      resultSlot.Init(support);
    }

    if (data.items.Length > 0) {
      foreach (Item item in data.items) {
        GameObject obj = Instantiate(GameManager.I.slotChain, componentsPanel);
        obj.GetComponent<SlotWithCount>().Init(item);
      }
    }

    if (data.equipment.Length > 0) {
      foreach (Equipment item in data.equipment) {
        GameObject obj = Instantiate(GameManager.I.slotWithCount, componentsPanel);
        obj.GetComponent<SlotWithCount>().Init(item);
      }
    }

    CheckEnoughResources();
  }

  public void CheckEnoughResources() {
    if (chain == null) return;
    Player player = Player.Instance;
    bool check = true;

    if (player.Gold < chain.cost) {
      check = false;
      price.text = $"<color=#F61010>{chain.cost}</color>";
    } else {
      price.text = chain.cost.ToString();
    }

    if (chain.sourceUnit != null) {
      if (!player.Army.HasUnit(chain.sourceUnit)) check = false;
    }

    if (chain.sourceSupport != null) {
      if (!player.Army.HasSupport(chain.sourceSupport.id)) check = false;
    }

    if (chain.sourceVillagersCount > 0) {
      if (player.Villagers < chain.sourceVillagersCount) check = false;
    }

    foreach (Item item in chain.items) {
      if (!player.Inventory.HasItem(item)) check = false;
    }

    foreach (Equipment item in chain.equipment) {
      if (!player.Inventory.HasItem(item, true)) check = false;
    }

    trainButton.interactable = check;
    if (check) arrowIcon.color = accessColor;
    else arrowIcon.color = denyColor;
  }

  private void Train() {
    if (chain == null) return;
    Player player = Player.Instance;

    foreach (Equipment item in chain.equipment) {
      if (!player.Inventory.HasItem(item, true)) {
        _ = Toast.Show("warning", "The required item is equipped on the unit");
        return;
      }
    }

    if (chain.sourceUnit != null) player.Army.DeleteUnit(chain.sourceUnit);
    else if (chain.sourceSupport != null) player.Army.DeleteSupport(chain.sourceSupport.id, chain.sourceSupportLevel);
    else if (chain.sourceVillagersCount > 0) player.SetVillagers(chain.sourceVillagersCount * -1);

    foreach (Equipment item in chain.equipment) player.Inventory.RemoveItem(item);
    foreach (Item item in chain.items) player.Inventory.RemoveItem(item);

    player.SetGold(chain.cost * -1);
    if (chain.resultUnit != null) player.Army.AddUnit(chain.resultUnit);
    else if (chain.resultSupport != null) player.Army.AddSupport(chain.resultSupport, chain.resultSupportLevel);

    _ = Toast.Show("success", "Unit is ready");
    MapUI.Instance.UpdateResources();
    HomeMenuUI.RecalculateRecipes();
  }
}
