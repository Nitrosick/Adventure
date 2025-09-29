using System;
using UnityEngine;
using UnityEngine.UI;

public class CollectingModalUI : ModalRewardUI {
  public static CollectingModalUI Instance;

  private static Button collect;
  private static Button cancel;

  private void Awake() {
    Instance = this;
  }

  private void Init() {
    window = transform.Find("Modals/Collecting");
    base.Init(window);

    collect = window.Find("Control/Collect").GetComponent<Button>();
    cancel = window.Find("Control/Cancel").GetComponent<Button>();

    if (window == null || collect == null || cancel == null) {
      Debug.LogError("Collecting dialog components initialization error");
      return;
    }

    collect.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(OnDecline);
  }

  private void OnDestroy() {
    if (collect != null) collect.onClick.RemoveListener(OnSubmit);
    if (cancel != null) cancel.onClick.RemoveListener(OnDecline);
  }

  protected override void Open() {
    base.Open();
    SceneController.OpenWindow("collecting-dialog");
  }

  protected override void Close() {
    base.Close();
    ClearSlots();
    SceneController.CloseWindow("collecting-dialog");
  }

  private void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public void Confirmation(Action<bool> action, Reward reward) {
    Init();
    callback = action;
    title.text = "Collecting";
    ShowReward(reward);
    RenderSlots(reward, Instance.slotPrefab);
    Open();
  }
}
