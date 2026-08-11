using System.Collections.Generic;
using System.Threading.Tasks;
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
    } else {
      List<Skill> skills = CurrentUnit.Equip.GetActiveSkills();
      BattleUI.Instance.ShowSkills(skills, PhaseManager.CurrentPhase, CurrentUnit);
      CurrentUnit.Ui.MarkAsActive();
    }
  }

  public void SortQueue() {
    Queue.Sort((a, b) => b.GetInitiative().CompareTo(a.GetInitiative()));
  }

  public async Task NextUnit() {
    if (Queue == null || Queue.Count == 0) return;
    await AdvanceOrder();
    Unit nextUnit = await GetNextAliveUnit();
    if (nextUnit == null) return;
    await SwitchTo(nextUnit);
  }

  private async Task<Unit> GetNextAliveUnit() {
    int safety = Queue.Count;

    while (safety-- > 0) {
      Unit unit = Queue[orderNumber];
      if (!unit.IsDead) return unit;
      await AdvanceOrder();
    }

    return null;
  }

  private async Task AdvanceOrder() {
    orderNumber++;

    if (orderNumber >= Queue.Count) {
      orderNumber = 0;
      Round++;
      SupportController.EveryTurn();
      await BattleManager.Instance.CheckReinforcement(Round);
      SortQueue();
    }
  }

  private async Task SwitchTo(Unit nextUnit) {
    HandleUI(nextUnit);
    BeforeSwitch();
    CurrentUnit = nextUnit;
    await AfterSwitch();
  }

  private void BeforeSwitch() {
    CurrentUnit.SetAttackType(AttackType.Standard);
    if (CurrentUnit.CurrentTile.type == TileType.Cover) {
      CurrentUnit.Animator.SetCrouching(true);
    }
    CurrentUnit.Ui.MarkAsInactive();
  }

  private async Task AfterSwitch() {
    CurrentUnit.Effects.ProcessTurnEffects();

    if (CurrentUnit.IsDead || CurrentUnit.Effects.PreventsTurn()) {
      await NextUnit();
      return;
    }

    CurrentUnit.ResetMovePoints();
    CurrentUnit.Animator.Reset();
    CurrentUnit.Ui.MarkAsActive();
    BattleUI.Instance.UpdateQueue(Queue, orderNumber);

    FocusOnUnit();

    if (CurrentUnit.Relation == UnitRelation.Enemy) {
      if (CurrentUnit.BehaviorType != AIBehaviorType.HoldPosition) return;
      int enemiesClose = BattleAIHeplers.CountEnemiesInRange(
        BattleAI.PlayerUnits(), CurrentUnit, 4
      );
      if (enemiesClose > 0) CurrentUnit.BehaviorType = AIBehaviorType.KeepDistance;
      return;
    }

    if (CurrentUnit.CurrentTile.type == TileType.Climb)
      BattleUI.Instance.ShowClimbButton();
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

    if (alliesCount == 0) BattleManager.Instance.battleResult = BattleResult.Defeat;
    else if (enemiesCount == 0) BattleManager.Instance.battleResult = BattleResult.Victory;

    if (BattleManager.Instance.battleResult != null) {
      BattleManager.Instance.Finish();
      return true;
    }
    return false;
  }
}
