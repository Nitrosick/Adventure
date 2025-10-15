using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapZoneEvent : MonoBehaviour {
  private MapZone zone;
  private Quest currentQuest;

  private void Awake() {
    zone = transform.GetComponent<MapZone>();
  }

  public void CheckEvents(
    bool ignoreBattle = false,
    bool forceAmbush = false,
    bool ignoreQuest = false
  ) {
    MapUI.Instance.HideInteractableButton();
    if (zone == null || zone.events.Count < 1) return;

    KnowledgeManager.UnlockArticle("aa4");

    T Get<T>() where T : Component => transform.GetComponent<T>();
    MapZoneBattle battleZone = Get<MapZoneBattle>();
    int eventIndex = 0;

    if (zone.events[eventIndex] == MapZoneType.Ambush) {
      eventIndex++;
      if (!ignoreBattle) {
        float chance = battleZone.ambushChance;
        chance -= AbilityController.AmbushProtectBonus();
        bool check = Utils.RollChance(chance);
        if (check || forceAmbush) {
          StartBattle(battleZone, true);
          return;
        }
      }
    } else if (zone.events[eventIndex] == MapZoneType.Quest && ignoreQuest) {
      eventIndex++;
    }

    switch (zone.events[eventIndex]) {
      case MapZoneType.Battle:
        if (ignoreBattle) return;
        if (battleZone.instant) StartBattle(battleZone);
        else MapUI.Instance.ShowInteractableButton(battleZone.StartBattle, "battle", "Attack");
        break;
      case MapZoneType.Quest:
        CheckZoneQuests(battleZone);
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
    }
  }

  private void CheckZoneQuests(MapZoneBattle battle) {
    List<Quest> list = zone.QuestsList;
    if (list.Count == 0) return;

    foreach (Quest q in list) {
      switch (q.objectiveType) {
        case QuestObjective.Fight:
          if (battle.instant) StartBattle(battle);
          else MapUI.Instance.ShowInteractableButton(battle.StartBattle, "battle", "Attack");
          return;
        case QuestObjective.VisitZone:
          QuestManager.CompleteQuest(q);
          list.Remove(q);
          if (list.Count == 0) zone.RemoveEvent(MapZoneType.Quest);
          return;
        case QuestObjective.BringItem:
          bool check = true;
          PlayerInventory inventory = Player.Instance.Inventory;
          if (q.objectiveEquipment.Length > 0 && !inventory.HasItems(q.objectiveEquipment, true)) check = false;
          if (q.objectiveItems.Length > 0 && !inventory.HasItems(q.objectiveItems)) check = false;
          if (!check) return;
          currentQuest = q;

          Dialog.Instance.Confirmation(
            HandInItems,
            "Quest items",
            $"Hand in {QuestManager.GetObjectiveItemsDescription(q)}?"
          );
          return;
      }
    }
  }

  private void HandInItems(bool accepted) {
    if (!accepted) {
      CheckEvents(ignoreQuest: true);
      return;
    }

    PlayerInventory inventory = Player.Instance.Inventory;
    inventory.RemoveItems(currentQuest.objectiveEquipment);
    inventory.RemoveItems(currentQuest.objectiveItems);
    QuestManager.CompleteQuest(currentQuest);
    zone.QuestsList.Remove(currentQuest);
    if (zone.QuestsList.Count == 0) zone.RemoveEvent(MapZoneType.Quest);
    currentQuest = null;
    CheckEvents();
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
    if (battleZone.reinforcement.Length > 0) StateManager.WriteUnitsData(battleZone.reinforcement, "reinforcement");
    StateManager.reinforcementRound = battleZone.reinforcementRound;
    StateManager.trapsCount = battleZone.trapsCount;

    MapUI.Instance.DisableUI();
    MapUI.Instance.HideZoneInfo();
    SceneController.ShowEventInfo("battle", isAmbush ? "Ambush!" : "Battle is starting");
    SceneController.SwitchScene(battleZone.battlefieldName);
  }
}
