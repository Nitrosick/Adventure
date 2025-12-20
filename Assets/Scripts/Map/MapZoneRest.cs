using UnityEngine;

public class MapZoneRest : MonoBehaviour {
  public void OpenRestDialog() {
    if (StateManager.currentWinStreak < 3) {
      _ = Toast.Show("warning", "No time to rest");
      return;
    }

    Dialog.Instance.Confirmation(
      TakeARest,
      "Make a stop",
      "You have fought several glorious battles and now deserve a rest. After the halt, the health of all units will be fully restored and the current win counter will be reset. Set up camp here?"
    );
  }

  private async void TakeARest(bool accepted) {
    if (!accepted) return;

    SceneController.ShowEventInfo("rest", "Rest");
    await SceneController.Fade(0f, 1f, true);

    StateManager.currentWinStreak = 0;
    foreach (Unit unit in Player.Instance.Army.Units) {
      unit.Health.Heal(-1, false);
    }
    MapUI.Instance.HideStatus("canRest");
    Player.Instance.Effects.AddBuff("b1");

    await SceneController.Fade(1f, 0f, false);
    SceneController.HideEventInfo();
    StateManager.SaveGame();
  }
}
