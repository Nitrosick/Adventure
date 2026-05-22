using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapZoneEvent : MonoBehaviour {
  private MapZone zone;

  void Awake() {
    zone = transform.GetComponent<MapZone>();
  }

  public void CheckEvents(
    bool ignoreBattle = false,
    bool forceAmbush = false,
    bool ignoreQuest = false
  ) {
    if (!ignoreQuest) QuestManager.CheckQuestsInZone(zone);
    MapUI.Instance.HideInteractableButton();
    if (zone == null || zone.events.Count < 1) return;
    KnowledgeManager.UnlockArticle("aa4");

    int order = 0;
    if (ignoreBattle && zone.events[0] == MapZoneType.Battle) order++;

    T Get<T>() where T : Component => transform.GetComponent<T>();

    switch (zone.events[order]) {
      case MapZoneType.Battle:
        MapZoneBattle battle = Get<MapZoneBattle>();
        if (battle == null) return;

        if (battle.ambushChance > 0) {
          float chance = battle.ambushChance;
          chance -= AbilityController.AmbushProtectBonus();
          chance -= SupportController.GetBonus("su2", false)[0];
          bool check = Randomiser.RollChance(chance);

          if (check || forceAmbush) StartBattle(battle, true);
          else CheckEvents(ignoreQuest: ignoreQuest);
        } else {
          if (battle.instant) StartBattle(battle);
          else MapUI.Instance.ShowInteractableButton(battle.StartBattle, "battle", "Attack");
        }
        break;
      case MapZoneType.Hub:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneHub>().OpenHubMenu
        );
        break;
      case MapZoneType.Recruitment:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneRecruitment>().OpenRecruitmentPanel
        );
        break;
      case MapZoneType.Constructing:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneBuilding>().OpenBuildingPanel
        );
        break;
      case MapZoneType.Task:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneTask>().OpenQuestModal
        );
        break;
      case MapZoneType.Collecting:
        MapZoneCollecting collecting = Get<MapZoneCollecting>();
        if (collecting.CollectedAt > 0 && StateManager.globalTicks < collecting.CollectedAt + collecting.respawn) break;

        MapUI.Instance.ShowInteractableButton(
          collecting.OpenCollectingPanel,
          "collect",
          "Collect"
        );
        break;
      case MapZoneType.Rest:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneRest>().OpenRestDialog,
          "rest",
          "Rest"
        );
        break;
    }
  }

  public void StartBattle(MapZoneBattle battleZone, bool isAmbush = false) {
    if (battleZone.guard == null || battleZone.guard.Length < 1) {
      Debug.LogError("Zone guard is not specified");
      return;
    }

    Unit[] playerUnits = Player.Instance.Army.Units.ToArray();
    Unit[] unitsInSquad = Player.Instance.Army.Units.Where(u => u.InSquad && u.CurrentHealth > 0).ToArray();

    if (playerUnits == null || playerUnits.Length == 0) {
      Debug.LogError("Player doesn't have an army");
      return;
    }

    if (unitsInSquad.Length > battleZone.armySlots) {
      bool cancelable = false;
      if (!isAmbush) cancelable = zone.events[0] != MapZoneType.Battle;
      SquadOverwhelmed.Open(battleZone.armySlots, this, cancelable, isAmbush);
      return;
    }

    StateManager.ResetTemp();
    StateManager.enterScene = SceneManager.GetActiveScene().name;

    StateManager.WriteUnitsData(playerUnits, "allies");
    StateManager.WriteUnitsData(battleZone.guard, "enemies");

    if (battleZone.reinforcement.Length > 0) StateManager.WriteUnitsData(battleZone.reinforcement, "reinforcement");
    StateManager.reinforcementRound = battleZone.reinforcementRound;

    StateManager.trapsCount = battleZone.trapsCount;
    StateManager.trapType = battleZone.trapType;

    MapUI.Instance.DisableUI();
    MapUI.Instance.HideZoneInfo();

    SceneController.ShowEventInfo("battle", isAmbush ? "Ambush!" : "Battle is starting");
    SceneController.SwitchScene(battleZone.battlefieldName);
  }
}
