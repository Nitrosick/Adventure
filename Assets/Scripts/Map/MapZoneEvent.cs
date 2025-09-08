using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapZoneEvent : MonoBehaviour {
  private MapZone zone;

  private void Awake() {
    zone = transform.GetComponent<MapZone>();
  }

  public void CheckEvents(bool ignoreBattle = false, bool forceAmbush = false) {
    if (zone == null || zone.events.Count < 1) return;
    T Get<T>() where T : Component => transform.GetComponent<T>();
    MapZoneBattle battleZone = Get<MapZoneBattle>();
    int eventIndex = 0;

    if (zone.events[eventIndex] == MapZoneType.Ambush) {
      eventIndex++;
      if (!ignoreBattle) {
        bool check = Utils.RollChance(battleZone.ambushChance);
        if (check || forceAmbush) {
          StartBattle(battleZone, true);
          return;
        }
      }
    }

    switch (zone.events[eventIndex]) {
      case MapZoneType.Battle:
        if (ignoreBattle) return;
        if (battleZone.instant) StartBattle(battleZone);
        else MapUI.Instance.ShowInteractableButton(battleZone.StartBattle);
        break;
      case MapZoneType.Quest:
        MapZoneQuest questZone = Get<MapZoneQuest>();
        if (questZone == null) break;
        CheckZoneQuests(questZone.questsList, battleZone);
        break;
      case MapZoneType.Home:
        MapUI.Instance.ShowInteractableButton(
          Get<MapZoneHome>().OpenHomeMenu
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
    }
  }

  private void CheckZoneQuests(List<Quest> quests, MapZoneBattle battle) {
    if (quests.Count == 0) return;

    foreach (Quest q in quests) {
      switch (q.objectiveType) {
        case QuestObjective.Fight:
          if (battle.instant) StartBattle(battle);
          else MapUI.Instance.ShowInteractableButton(battle.StartBattle, "battle", "Attack");
          return;
        case QuestObjective.VisitZone:
          QuestManager.CompleteQuest(q);
          quests.Remove(q);
          if (quests.Count == 0) zone.UnshiftEvent();
          return;
        case QuestObjective.BringItem:
          // FIXME: Добавить проверку (возможно открывать диалог о сдаче предмета)
          return;
      }
    }
  }

  public void StartBattle(MapZoneBattle battleZone, bool isAmbush = false) {
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
      bool cancelable = false;
      if (!isAmbush) cancelable = zone.events[0] != MapZoneType.Battle;
      SquadOverwhelmed.Open(battleZone.armySlots, this, cancelable, isAmbush);
      return;
    }

    StateManager.ResetTemp();
    StateManager.enterScene = SceneManager.GetActiveScene().name;
    StateManager.WriteUnitsData(playerUnits, "allies");
    StateManager.WriteUnitsData(battleZone.guard, "enemies");
    StateManager.trapsCount = battleZone.trapsCount;

    MapUI.Instance.DisableUI();
    MapUI.Instance.HideZoneInfo();
    SceneController.ShowEventInfo("battle", isAmbush ? "Ambush!" : "Battle is starting");
    SceneController.SwitchScene(battleZone.battlefieldName);
  }
}
