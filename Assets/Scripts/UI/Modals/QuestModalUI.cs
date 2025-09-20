using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestModalUI : ModalRewardUI {
  public static QuestModalUI Instance;

  private static Button accept;
  private static Button cancel;
  private static TextMeshProUGUI cancelText;
  private static QuestInstance quest;

  private void Awake() {
    Instance = this;
  }

  private void Init() {
    window = transform.Find("Modals/Quest");
    base.Init(window);

    accept = window.Find("Control/Accept").GetComponent<Button>();
    cancel = window.Find("Control/Cancel").GetComponent<Button>();
    cancelText = window.Find("Control/Cancel/Text").GetComponent<TextMeshProUGUI>();

    if (window == null || accept == null || cancel == null || cancelText == null) {
      Debug.LogError("Quest dialog components initialization error");
      return;
    }

    accept.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(OnDecline);
  }

  private void OnDestroy() {
    if (accept != null) accept.onClick.RemoveListener(OnSubmit);
    if (cancel != null) cancel.onClick.RemoveListener(OnDecline);
  }

  protected override void Open() {
    base.Open();
    SceneController.OpenWindow("quest-dialog");
  }

  protected override void Close() {
    base.Close();
    accept.gameObject.SetActive(true);
    ClearSlots();
    SceneController.CloseWindow("quest-dialog");
  }

  private void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public void Acception(Action<bool> action, QuestInstance _quest) {
    Init();
    callback = action;
    quest = _quest;
    title.text = quest.data.title;
    if (quest.state == QuestState.Completed) title.text += " (Completed)";

    text.text = quest.state == QuestState.Completed
     ? quest.data.descriptionCompleted
     : quest.data.description;

    Reward reward = quest.data.reward;
    ShowReward(reward);
    RenderSlots(reward, Instance.slotPrefab);

    bool acceptable = quest.state == QuestState.Inactive;
    accept.gameObject.SetActive(acceptable);
    cancelText.text = acceptable ? "Cancel" : "Close";
    Open();
  }

  public void ShowReward(QuestInstance _quest) {
    Init();
    quest = _quest;
    title.text = quest.data.title;
    text.text = "Quest completed!";

    Reward reward = quest.data.reward;
    ShowReward(reward);
    RenderSlots(reward, Instance.slotPrefab);

    accept.gameObject.SetActive(false);
    cancelText.text = "Get reward";
    Open();
  }
}
