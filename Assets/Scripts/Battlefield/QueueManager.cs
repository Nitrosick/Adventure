using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour {
  public static QueueManager Instance;

  public List<Unit> Queue { get; private set; } = new();
  public Unit CurrentUnit { get; private set; }
  public int Round { get; private set; } = 1;
  private int orderNumber;

  void Awake() {
    Instance = this;
  }

  void OnDestroy() {
    Queue.Clear();
    CurrentUnit = null;
    Round = 1;
    orderNumber = 0;
  }

  public void Init() {
    if (Queue.Count < 1) {
      Debug.LogError("No units have been added to the queue");
      return;
    }

    SortQueue();
    orderNumber = 0;
    CurrentUnit = Queue[0];
    FocusOnUnit();
    BattleUI.Instance.UpdateQueue(Queue);

    if (CurrentUnit.Relation == UnitRelation.Enemy) {
      BattleAI.Init(CurrentUnit);
      BattleAI.EnemyMove();
    }
    else {
      List<Skill> skills = CurrentUnit.Equip.GetActiveSkills();
      BattleUI.Instance.ShowSkills(skills, PhaseManager.CurrentPhase, CurrentUnit);
      CurrentUnit.Ui.MarkAsActive();
    }
  }

  public void SortQueue() {
    Queue.Sort((a, b) => b.GetInitiative().CompareTo(a.GetInitiative()));
  }

  public void NextUnit() {
    if (Queue == null || Queue.Count == 0) return;
    AdvanceOrder();
    Unit nextUnit = GetNextAliveUnit();
    if (nextUnit == null) return;
    SwitchTo(nextUnit);
  }

  private Unit GetNextAliveUnit() {
    int safety = Queue.Count;

    while (safety-- > 0) {
      Unit unit = Queue[orderNumber];
      if (!unit.IsDead) return unit;
      AdvanceOrder();
    }

    return null;
  }

  private async void AdvanceOrder() {
    orderNumber++;

    if (orderNumber >= Queue.Count) {
      orderNumber = 0;
      Round++;
      // FIXME: Не работает асинхронность
      await SupportController.EveryTurn();
      await BattleManager.Instance.CheckReinforcement(Round);
      SortQueue();
    }
  }

  private void SwitchTo(Unit nextUnit) {
    HandleUI(nextUnit);
    BeforeSwitch();
    CurrentUnit = nextUnit;
    AfterSwitch();
  }

  private void BeforeSwitch() {
    CurrentUnit.SetAttackType(AttackType.Standard);
    if (CurrentUnit.CurrentTile.type == TileType.Cover) {
      CurrentUnit.Animator.SetCrouching(true);
    }
    CurrentUnit.Ui.MarkAsInactive();
  }

  private void AfterSwitch() {
    CurrentUnit.Effects.ProcessTurnEffects();

    if (CurrentUnit.IsDead || CurrentUnit.Effects.PreventsTurn()) {
      NextUnit();
      return;
    }

    CurrentUnit.ResetMovePoints();
    CurrentUnit.Animator.Reset();
    CurrentUnit.Ui.MarkAsActive();
    BattleUI.Instance.UpdateQueue(Queue, orderNumber);

    if (
      CurrentUnit.Relation != UnitRelation.Enemy &&
      CurrentUnit.CurrentTile.type == TileType.Climb
    ) BattleUI.Instance.ShowClimbButton();

    FocusOnUnit();
  }

  private void HandleUI(Unit unit) {
    if (unit.Relation == UnitRelation.Enemy) {
      BattleUI.Instance.DisableUI();
    }
    else {
      BattleUI.Instance.EnableUI();
      _ = Toast.Show("move", "Movement phase", 1);
    }
  }

  private void FocusOnUnit() {
    TileManager.ShowReachableTiles(
      CurrentUnit.CurrentTile,
      CurrentUnit.CurrentMovePoints
    );

    _ = CameraController.FocusOn(CurrentUnit.transform.position);
  }

  public bool CheckBattleIsOver() {
    int alliesCount = 0;
    int enemiesCount = 0;

    foreach (Unit unit in Queue) {
      if (unit.IsDead) continue;
      if (unit.Relation == UnitRelation.Ally) alliesCount++;
      else if (unit.Relation == UnitRelation.Enemy) enemiesCount++;
    }

    if (alliesCount == 0) BattleManager.battleResult = BattleResult.Defeat;
    else if (enemiesCount == 0) BattleManager.battleResult = BattleResult.Victory;

    if (BattleManager.battleResult != null) {
      BattleManager.Finish();
      return true;
    }
    return false;
  }
}
