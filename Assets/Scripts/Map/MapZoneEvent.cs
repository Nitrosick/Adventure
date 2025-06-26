using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapZoneEvent : MonoBehaviour
{
  private MapZone zone;

  private void Awake() {
    zone = transform.GetComponent<MapZone>();
  }

  public void CheckEvents() {
    if (zone == null) return;

    if (zone.guard != null && zone.guard.Length > 0) StartBattle();
  }

  private void StartBattle() {
    Unit[] allies = Player.Instance.Army.Units
      .Where(u => u.InSquad).ToArray();
    Unit[] reserve = Player.Instance.Army.Units
      .Where(u => !u.InSquad).ToArray();

    if (allies == null || allies.Length == 0) {
      Debug.LogError("Player doesn't have an army");
      // FIXME: Открыть окно перераспределения войск
      return;
    }

    if (allies.Length > zone.armySlots) {
      Debug.Log("Too many units for this battle");
      // FIXME: Открыть окно перераспределения войск
      return;
    }

    StateManager.Reset();
    StateManager.enterScene = SceneManager.GetActiveScene().name;
    StateManager.WriteUnitsData(allies, "allies");
    StateManager.WriteUnitsData(reserve, "reserve");
    StateManager.WriteUnitsData(zone.guard, "enemies");
    SceneController.ShowEventInfo("battle", "Battle is starting");
    SceneController.SwitchScene(zone.battlefieldName);
  }
}
