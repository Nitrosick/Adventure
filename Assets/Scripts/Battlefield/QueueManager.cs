using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
  public static List<Unit> Queue { get; private set; } = new ();
  public static Unit CurrentUnit { get; private set; }
  public static int Round { get; private set; } = 1;
  private static int orderNumber;

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
    BattleUI.Instance.UpdateQueue(Queue);
    List<Skill> skills = CurrentUnit.Equip.GetActiveSkills();
    if (CurrentUnit.Relation != UnitRelation.Enemy) BattleUI.Instance.ShowSkills(skills, PhaseManager.CurrentPhase, CurrentUnit);
    CurrentUnit.Ui.MarkAsActive();
    FocusOnUnit();

    if (CurrentUnit.Relation == UnitRelation.Enemy) BattleAI.EnemyMove(CurrentUnit);
  }

  public static void NextUnit() {
    if (orderNumber >= Queue.Count - 1) {
      orderNumber = 0;
      Round++;
      SupportController.EveryTurn();
    } else {
      orderNumber++;
    }

    Unit nextUnit = Queue[orderNumber];

    if (nextUnit.IsDead) {
      NextUnit();
      return;
    }

    if (nextUnit.Relation == UnitRelation.Enemy) BattleUI.Instance.DisableUI();
    else BattleUI.Instance.EnableUI();

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
    BattleUI.Instance.UpdateQueue(Queue, orderNumber);
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
      else if (unit.Relation == UnitRelation.Enemy) enemiesCount++;
    }

    if (alliesCount == 0) BattleManager.battleResult = BattleResult.Defeat;
    else if (enemiesCount == 0) BattleManager.battleResult = BattleResult.Victory;

    if (BattleManager.battleResult != null) BattleManager.Finish();
  }
}
