using System.Collections.Generic;
using UnityEngine;

public class TrainingMenuUI : HomeMenuFeature {
  public GameObject chainPrefab;
  private Transform soldiersPanel;
  private Transform supportsPanel;
  protected Dictionary<Rarity, Color> rarityPalette = new();

  protected override void Awake() {
    base.Awake();

    T Get<T>(string path) where T : Component => transform.Find(path).GetComponent<T>();

    soldiersPanel = Get<Transform>("Soldiers");
    supportsPanel = Get<Transform>("Supports");

    if (!ComponentsInitialized()) {
      Debug.LogError("Training menu UI components initialization error");
      return;
    }

    rarityPalette = Utils.GetRarityPalette();
  }

  private bool ComponentsInitialized() {
    return soldiersPanel != null && supportsPanel != null;
  }

  public void Init(string name, MasteryLevel lvl, TrainingChain[] soldiers, TrainingChain[] supports) {
    InitHeader(name, lvl);
    RenderChains(soldiers, soldiersPanel);
    RenderChains(supports, supportsPanel);
  }

  private void RenderChains(TrainingChain[] chains, Transform panel) {
    foreach (TrainingChain chain in chains) {
      GameObject obj = Instantiate(chainPrefab, panel);
      obj.GetComponent<TrainingChainUI>().Init(chain);
    }
  }

  public override void Clear() {
    base.Clear();

    if (!ComponentsInitialized()) return;
    ClearSlots(soldiersPanel);
    ClearSlots(supportsPanel);
  }
}
