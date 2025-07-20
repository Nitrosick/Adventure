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
    if (zone == null || zone.events.Count < 1) return;

    switch (zone.events[0]) {
      case MapZoneType.InstantBattle:
        StartBattle();
        break;
      case MapZoneType.Recruitment:
        MapZoneRecruitment component = transform.GetComponent<MapZoneRecruitment>();
        if (component == null) return;
        MapUI.ShowInteractableButton(component.OpenRecruitmentPanel);
        break;
    }
  }

  private void StartBattle() {
    if (zone is not MapZoneBattle battleZone) return;
    if (battleZone.guard == null || battleZone.guard.Length < 1) {
      Debug.LogError("Zone guard is not specified");
      return;
    }

    Unit[] playerUnits = Player.Instance.Army.Units.ToArray();
    Unit[] unitsInSquad = Player.Instance.Army.Units.Where(u => u.InSquad).ToArray();

    if (playerUnits == null || playerUnits.Length == 0) {
      Debug.LogError("Player doesn't have an army");
      return;
    }

    if (unitsInSquad.Length > battleZone.armySlots) {
      SquadOverwhelmed.Open(battleZone.armySlots, this, battleZone.events[0] != MapZoneType.InstantBattle);
      return;
    }

    StateManager.ResetTemp();
    StateManager.enterScene = SceneManager.GetActiveScene().name;
    StateManager.WriteUnitsData(playerUnits, "allies");
    StateManager.WriteUnitsData(battleZone.guard, "enemies");

    MapUI.DisableUI();
    MapUI.HideZoneInfo();
    SceneController.ShowEventInfo("battle", "Battle is starting");
    SceneController.SwitchScene(battleZone.battlefieldName);
  }
}
