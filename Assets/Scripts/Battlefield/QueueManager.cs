using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class QueueManager : MonoBehaviour
{
  public static List<Unit> Queue { get; private set; } = new ();
  public static Unit CurrentUnit { get; private set; }
  public static int Round { get; private set; } = 1;
  private static int orderNumber;

  private void Update() {
    if (
      BattleManager.battleResult != null ||
      CurrentUnit == null ||
      !Mouse.current.leftButton.wasPressedThisFrame
    ) return;

    switch (PhaseManager.CurrentPhase) {
      case BattlePhase.Movement:
        if (!CurrentUnit.GetComponent<UnitMove>().IsMoving) HandleClick();
        break;
      case BattlePhase.Attack:
        HandleClick();
        break;
      // case BattlePhase.Ability:
      //   break;
    }
  }

  private void OnDestroy() {
    Queue.Clear();
    CurrentUnit = null;
    Round = 1;
    orderNumber = 0;
  }

  public static void Init() {
    if (Queue.Count < 1) {
      Debug.LogError("No units have been added to the queue");
      return;
    }

    Queue.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));
    orderNumber = 0;
    CurrentUnit = Queue[0];
    BattleUI.UpdateQueue(Queue);
    List<Skill> skills = CurrentUnit.Equip.GetSkills();
    if (CurrentUnit.Relation != UnitRelation.Emeny) BattleUI.ShowSkills(skills, PhaseManager.CurrentPhase, CurrentUnit);
    CurrentUnit.Ui.MarkAsActive();
    FocusOnUnit();

    if (CurrentUnit.Relation == UnitRelation.Emeny) BattleAI.EnemyMove(CurrentUnit);
  }

  public static void NextUnit() {
    orderNumber = (orderNumber + 1) % Queue.Count;
    // if (orderNumber >= Queue.Count - 1) {
    //   orderNumber = 0;
    //   Round++;
    // }
    // else {
    //   orderNumber++;
    // }

    Unit nextUnit = Queue[orderNumber];

    if (nextUnit.IsDead) {
      NextUnit();
      return;
    }

    if (nextUnit.Relation == UnitRelation.Emeny) BattleUI.DisableUI();
    else BattleUI.EnableUI();

    BeforeSwitch();
    CurrentUnit = nextUnit;
    AfterSwitch();
  }

  private static void BeforeSwitch() {
    if (CurrentUnit.CurrentTile.type == TileType.Cover) {
      CurrentUnit.Animator.SetCrouching(true);
    }
    CurrentUnit.Ui.MarkAsInactive();
  }

  private static void AfterSwitch() {
    if (CurrentUnit.Effects.PreventsTurn()) {
      CurrentUnit.Effects.ProcessTurnEffects();
      NextUnit();
      return;
    }

    CurrentUnit.Effects.ProcessTurnEffects();
    CurrentUnit.Animator.Reset();
    CurrentUnit.Ui.MarkAsActive();
    BattleUI.UpdateQueue(Queue, orderNumber);
    FocusOnUnit();
  }

  private static void FocusOnUnit() {
    TileManager.ShowReachableTiles(
      CurrentUnit.CurrentTile,
      CurrentUnit.CurrentMovePoints
    );

    _ = CameraController.FocusOn(CurrentUnit.transform.position);
  }

  public static void CheckBattleIsOver() {
    int alliesCount = 0;
    int enemiesCount = 0;

    foreach (Unit unit in Queue) {
      if (unit.IsDead) continue;
      if (unit.Relation == UnitRelation.Ally) alliesCount++;
      else if (unit.Relation == UnitRelation.Emeny) enemiesCount++;
    }

    if (alliesCount == 0) BattleManager.battleResult = BattleResult.Defeat;
    else if (enemiesCount == 0) BattleManager.battleResult = BattleResult.Victory;

    if (BattleManager.battleResult != null) BattleManager.Finish();
  }

  private void HandleClick() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out RaycastHit hit)) {
      string tag = hit.collider.gameObject.tag;

      switch (tag) {
        case "Unit":
          if (hit.collider.TryGetComponent<Unit>(out var clickedUnit)) {
            if (
              CurrentUnit == clickedUnit ||
              CurrentUnit.Relation == clickedUnit.Relation ||
              !clickedUnit.CurrentTile.AttackGrid.activeSelf
            ) return;

            TileManager.HideGrid();
            CurrentUnit.OnAttack(clickedUnit);
          }
          break;

        case "BattlefieldTile":
          if (hit.collider.TryGetComponent<Tile>(out var clickedTile)) {
            if (
              clickedTile == null ||
              !TileManager.TileIsWalkable(clickedTile) ||
              !clickedTile.Grid.activeSelf
            ) return;

            TileManager.HideGrid();
            CurrentUnit.GetComponent<UnitMove>().OnMove(clickedTile);
          }
          break;

        case "Breakable":
          if (PhaseManager.CurrentPhase != BattlePhase.Attack || !CurrentUnit.Equip.CanBreakObjects()) return;

          if (hit.collider.TryGetComponent<Breakable>(out var clickedObj)) {
            if (clickedObj == null || !clickedObj.ParentTile.AttackGrid.activeSelf) return;
            TileManager.HideGrid();
            CurrentUnit.BreakObject(clickedObj);
          }
          break;
      }
    }
  }
}
