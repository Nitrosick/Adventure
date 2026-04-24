using System.Linq;
using UnityEngine;

public class LockedObject : MonoBehaviour {
  public string id;
  public string title;
  public MapZone parentZone;
  public LockDifficulty difficulty;
  public Item key;
  public float catchChance;
  public Reward reward;

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

    tooltip.message = $"<b>{title}</b>\n{difficulty} lock";
    opened = StateManager.openedLocks.Contains(id);
  }

  public void Open() {
    if (opened || player.Move.CurrentZone.id != parentZone.id) return;

    if (key != null) {
      if (player.Inventory.HasItem(key)) {
        GetReward();
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

    bool isDay = TimeController.Instance.IsDay();
    breakingChance = SupportController.GetBonus("su7", false)[0];
    breakingChance -= breakingPenalty[(int)difficulty];
    if (isDay) breakingChance /= 2;

    string timeWarning = isDay ? "Breaking locks during the day is quite dangerous\n" : "";
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
        ? catchChance * 2
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
      GetReward();
    } else {
      player.Inventory.RemoveItem("t5");
      _ = Toast.Show("warning", "The lockpick is broken");
    }
  }

  private void GetReward() {
    breakingChance = 0f;
    burglar = null;

    player.CollectReward(reward);
    _ = Toast.Show("success", "The lock has been successfully opened");
    StateManager.openedLocks.Add(id);
    SetOpened();
  }

  public void SetOpened() {
    opened = true;
    tooltip.message = title;
  }
}
