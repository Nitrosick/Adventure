using System.Linq;
using UnityEngine;

public class LockedObject : MonoBehaviour {
  public string id;
  public string title;
  public string altText;
  public MapZone parentZone;
  public LockDifficulty difficulty;
  public Item key;
  public float catchChance;
  public Reward reward;
  public GameObject[] interactiveObjects = {};

  private Player player;
  private SupportInstance burglar;
  private TooltipTrigger tooltip;
  private float breakingChance;
  private bool opened;

  private readonly float[] breakingPenalty = { 0, 10, 20, 30, 50, 100 };
  private readonly int repPenalty = -15;

  void Start() {
    player = Player.Instance;
    tooltip = transform.GetComponent<TooltipTrigger>();

    if (parentZone == null || player == null || tooltip == null) {
      Debug.LogError("Locked object components initialization error");
    }

    tooltip.message = difficulty == LockDifficulty.Unlockable
      ? $"<b>{title}</b>\nNeed a key"
      : $"<b>{title}</b>\n{difficulty} lock";

    opened = StateManager.openedLocks.Contains(id);
    if (opened) SwitchObjects();
  }

  public void Open() {
    if (opened || player.Move.CurrentZone.id != parentZone.id) return;

    if (key != null) {
      if (player.Inventory.HasItem(key)) {
        player.Inventory.RemoveItem(key);
        SwitchObjects();
        GetReward();
        return;
      } else if (difficulty == LockDifficulty.Unlockable) {
        _ = Toast.Show("warning", "The lock is too complex, you need a key");
        return;
      }
    }

    burglar = player.Army.Supports
      .FirstOrDefault(s => s.data.id == "su7" && s.inSquad);
    if (burglar == null) {
      _ = Toast.Show("warning", "There is no unit in the squad capable of breaking this lock");
      return;
    }

    bool haveLockpick = player.Inventory.HasItem("t5");
    if (!haveLockpick) {
      _ = Toast.Show("warning", "You don't have lock picks");
      return;
    }

    breakingChance = SupportController.GetBonus("su7", false)[0];
    breakingChance -= breakingPenalty[(int)difficulty];

    string timeWarning = TimeController.Instance.IsDay() ? "Breaking locks during the day is quite dangerous\n" : "";
    string chanceWarning = breakingChance < 25 ? "This lock is too complex, the chance of picking it is very small\n" : "";
    string catchWarning = catchChance > 10 ? "In this place, you can easily be seen by people\n" : "";

    Dialog.Instance.Confirmation(
      TryBreak,
      "Breaking the lock",
      "Do you want to try picking this lock? If you fail, you will break the lock pick",
      timeWarning + chanceWarning + catchWarning
    );
  }

  private void TryBreak(bool accepted) {
    if (!accepted) return;

    bool caught = Randomiser.RollChance(
      TimeController.Instance.IsDay()
        ? catchChance * 4
        : catchChance
    );

    if (caught) {
      player.Army.DeleteSupport(burglar.data.id, burglar.level);
      player.SetReputation(repPenalty);
      LogUI.Instance.Add($"{repPenalty} Reputation");

      _ = Toast.Show("warning", "You have been caught stealing");
      StateManager.SaveGame();
      return;
    }

    bool success = Randomiser.RollChance(breakingChance);

    if (success) {
      SwitchObjects();
      GetReward();
    } else {
      player.Inventory.RemoveItem("t5");
      _ = Toast.Show("warning", "The lockpick is broken");
    }
  }

  private void SwitchObjects() {
    foreach (GameObject obj in interactiveObjects) {
      obj.SetActive(!obj.activeSelf);
    }
  }

  private void GetReward() {
    breakingChance = 0f;
    burglar = null;

    player.CollectReward(reward);
    string text = altText ?? "The lock has been successfully opened";
    _ = Toast.Show("success", text);
    StateManager.openedLocks.Add(id);
    SetOpened();
  }

  public void SetOpened() {
    opened = true;
    tooltip.message = title;
  }
}
