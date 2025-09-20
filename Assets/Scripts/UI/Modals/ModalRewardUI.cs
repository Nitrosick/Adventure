using System.Linq;
using TMPro;
using UnityEngine;

public class ModalRewardUI : ModalUI {
  public GameObject slotPrefab;

  private static GameObject rewardXPRow;
  private static TextMeshProUGUI rewardXP;
  private static GameObject rewardFameRow;
  private static TextMeshProUGUI rewardFame;
  private static GameObject rewardGoldRow;
  private static TextMeshProUGUI rewardGold;
  private static GameObject rewardStatPointsRow;
  private static TextMeshProUGUI rewardStatPoints;
  private static GameObject rewardAbilityPointsRow;
  private static TextMeshProUGUI rewardAbilityPoints;
  private static Transform rewardSlots;

  private static readonly int slotsInRow = 5;

  protected override void Init(Transform _window) {
    base.Init(_window);

    Transform Find(string path) => window.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    rewardXPRow = Find("Reward/Experience").gameObject;
    rewardXP = Get<TextMeshProUGUI>("Reward/Experience/Value");
    rewardFameRow = Find("Reward/Fame").gameObject;
    rewardFame = Get<TextMeshProUGUI>("Reward/Fame/Value");
    rewardGoldRow = Find("Reward/Gold").gameObject;
    rewardGold = Get<TextMeshProUGUI>("Reward/Gold/Value");
    rewardStatPointsRow = Find("Reward/StatPoints").gameObject;
    rewardStatPoints = Get<TextMeshProUGUI>("Reward/StatPoints/Value");
    rewardAbilityPointsRow = Find("Reward/AbilityPoints").gameObject;
    rewardAbilityPoints = Get<TextMeshProUGUI>("Reward/AbilityPoints/Value");
    rewardSlots = Find("Reward/Slots");

    if (
      rewardXP == null || rewardFame == null || rewardGold == null ||
      rewardStatPoints == null || rewardAbilityPoints == null || rewardSlots == null ||
      rewardXPRow == null || rewardFameRow == null || rewardGoldRow == null ||
      rewardStatPointsRow == null || rewardAbilityPointsRow == null
    ) {
      Debug.LogError("Modal components initialization error");
    }
  }

  protected void ClearSlots() {
    foreach (Transform child in rewardSlots) Destroy(child.gameObject);
  }

  protected void ShowReward(Reward reward) {
    rewardXPRow.SetActive(reward.experience > 0);
    if (reward.experience > 0) rewardXP.text = reward.experience.ToString();

    rewardFameRow.SetActive(reward.fame > 0);
    if (reward.fame > 0) rewardFame.text = reward.fame.ToString();

    rewardGoldRow.SetActive(reward.goldRange[1] > 0);
    if (reward.goldRange[1] > 0) rewardGold.text = reward.goldRange[0] == reward.goldRange[1]
      ? reward.goldRange[0].ToString()
      : $"{reward.goldRange[0]} - {reward.goldRange[1]}";

    rewardStatPointsRow.SetActive(reward.statPoints > 0);
    if (reward.statPoints > 0) rewardStatPoints.text = reward.statPoints.ToString();

    rewardAbilityPointsRow.SetActive(reward.abilityPoints > 0);
    if (reward.abilityPoints > 0) rewardAbilityPoints.text = reward.abilityPoints.ToString();
  }

  protected void RenderSlots(Reward reward, GameObject prefab) {
    int slotsCount = 0;

    rewardSlots.gameObject.SetActive(
      reward.resources.Any(x => x > 0) ||
      reward.equipment.Count > 0 ||
      reward.items.Count > 0
    );

    for (int i = 0; i < reward.resources.Length; i++) {
      if (reward.resources[i] > 0) {
        GameObject slot = Instantiate(prefab, rewardSlots);
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
        GameObject slot = Instantiate(prefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    if (reward.items.Count > 0) {
      foreach (Item item in reward.items) {
        GameObject slot = Instantiate(prefab, rewardSlots);
        slot.GetComponent<SlotWithCount>().Init(item);
        slotsCount++;
      }
    }

    RenderEmptySlots(slotsCount);
  }

  protected void RenderEmptySlots(int filled) {
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
