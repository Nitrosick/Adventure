using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestAcceptDialog : MonoBehaviour {
  public static QuestAcceptDialog Instance;
  public GameObject slotPrefab;

  private static Transform window;
  private static GameObject background;
  private static Button accept;
  private static Button cancel;
  private static Action<bool> callback;
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI description;
  private static Quest quest;

  // Reward
  private static TextMeshProUGUI rewardXP;
  private static TextMeshProUGUI rewardFame;
  private static TextMeshProUGUI rewardGold;
  private static Transform rewardSlots;

  private static readonly int slotsInRow = 5;

  private void Awake() {
    Instance = this;

    Transform Find(string path) => window.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    window = transform.Find("QuestAcceptDialog/Panel");
    background = transform.Find("QuestAcceptDialog/Background").gameObject;
    accept = Get<Button>("Control/Accept");
    cancel = Get<Button>("Control/Cancel");
    title = Get<TextMeshProUGUI>("Head/Title");
    description = Get<TextMeshProUGUI>("Text");
    rewardXP = Get<TextMeshProUGUI>("Reward/Experience/Value");
    rewardFame = Get<TextMeshProUGUI>("Reward/Fame/Value");
    rewardGold = Get<TextMeshProUGUI>("Reward/Gold/Value");
    rewardSlots = Find("Reward/Slots");

    if (
      window == null || background == null || accept == null ||
      cancel == null || title == null || description == null ||
      rewardSlots == null || rewardXP == null || rewardFame == null ||
      rewardGold == null
    ) {
      Debug.LogError("Quest dialog components initialization error");
      return;
    }

    accept.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(OnDecline);
  }

  private void OnDestroy() {
    accept.onClick.RemoveListener(OnSubmit);
    cancel.onClick.RemoveListener(OnDecline);
  }

  private static void Open() {
    window.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.OpenWindow("quest-dialog");
  }

  private static void Close() {
    callback = null;
    window.gameObject.SetActive(false);
    accept.gameObject.SetActive(true);
    background.SetActive(false);
    title.text = "";
    description.text = "";
    ClearSlots();
    SceneController.CloseWindow("quest-dialog");
  }

  private static void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private static void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public static void Acception(Action<bool> action, Quest _quest) {
    callback = action;
    quest = _quest;
    title.text = quest.title;
    description.text = quest.description;
    ShowReward();
    RenderSlots();
    Open();
  }

  private static void ClearSlots() {
    foreach (Transform child in rewardSlots) Destroy(child.gameObject);
  }

  private static void ShowReward() {
    Reward reward = quest.reward;
    if (reward.experience > 0) rewardXP.text = reward.experience.ToString();
    if (reward.fame > 0) rewardFame.text = reward.fame.ToString();
    if (reward.goldRange[1] > 0) rewardGold.text = reward.goldRange[0] == reward.goldRange[1]
      ? reward.goldRange[0].ToString()
      : $"{reward.goldRange[0]} - {reward.goldRange[1]}";
  }

  private static void RenderSlots() {
    Reward reward = quest.reward;
    int slotsCount = 0;

    rewardSlots.gameObject.SetActive(
      reward.resources.Any(x => x > 0) ||
      reward.equipment.Count > 0 ||
      reward.items.Count > 0
    );

    for (int i = 0; i < reward.resources.Length; i++) {
      if (reward.resources[i] > 0) {
        GameObject slot = Instantiate(Instance.slotPrefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(
          MapUI.Instance.resourceSprites[i],
          reward.resources[i],
          MapUI.Instance.resTooltips[i]
        );
        slotsCount++;
      }
    }

    if (reward.equipment.Count > 0) {
      foreach (Equipment item in reward.equipment) {
        GameObject slot = Instantiate(Instance.slotPrefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    if (reward.items.Count > 0) {
      foreach (Item item in reward.items) {
        GameObject slot = Instantiate(Instance.slotPrefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    RenderEmptySlots(slotsCount);
  }

  private static void RenderEmptySlots(int filled) {
    if (filled == slotsInRow) {
      return;
    } else if (filled < slotsInRow) {
      for (int i = filled; i < slotsInRow; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, rewardSlots);
      }
    } else {
      int remainder = filled % slotsInRow;
      int placeholders = remainder == 0 ? 0 : slotsInRow - remainder;

      for (int i = 0; i < placeholders; i++) {
        Instantiate(MapUI.Instance.emptySlotPrefab, rewardSlots);
      }
    }
  }
}
