using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QueueManager : MonoBehaviour {
  public static QueueManager Instance;
  private BattleUI UI => BattleUI.Instance;
  public List<Unit> Queue { get; private set; } = new();
  public Unit CurrentUnit { get; private set; }
  public int Round { get; private set; } = 1;
  private int orderNumber;

  void Awake() {
    Instance = this;
  }

  void OnDestroy() {
    Instance = null;
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
    UI.UpdateQueue(Queue);

    if (CurrentUnit.Relation == UnitRelation.Enemy) {
      BattleAI.Init(CurrentUnit);
      BattleAI.EnemyMove();
    }
    else {
      ShowUnitUI();
    }
  }

  public void SortQueue() {
    Queue.Sort((a, b) => b.GetInitiative().CompareTo(a.GetInitiative()));
  }

  public async Task NextUnit() {
    if (Queue.Count == 0) return;
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
    DeactivateCurrentUnit();
    CurrentUnit = nextUnit;
    HandleUI(CurrentUnit);
    await ActivateNextUnit();
  }

  private void DeactivateCurrentUnit() {
    CurrentUnit.SetAttackType(AttackType.Standard);
    if (CurrentUnit.CurrentTile.type == TileType.Cover) {
      CurrentUnit.Animator.SetCrouching(true);
    }
    CurrentUnit.Ui.MarkAsInactive();
  }

  private async Task ActivateNextUnit() {
    CurrentUnit.Effects.ProcessTurnEffects();

    if (CurrentUnit.IsDead || CurrentUnit.Effects.PreventsTurn()) {
      await NextUnit();
      return;
    }

    CurrentUnit.ResetMovePoints();
    CurrentUnit.Animator.Reset();
    CurrentUnit.Ui.MarkAsActive();
    UI.UpdateQueue(Queue, orderNumber);

    ShowUnitUI();
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
      UI.ShowClimbButton();
    else
      UI.HideClimbButton();
  }

  private void HandleUI(Unit unit) {
    bool isPlayer = unit.Relation == UnitRelation.Ally;

    if (isPlayer) {
      UI.EnableUI();
      _ = Toast.Show("move", "Movement phase", 1);
    }
    else {
      UI.DisableUI();
    }
  }

  private void ShowUnitUI() {
    CurrentUnit.Ui.MarkAsActive();

    UI.ShowSkills(
      CurrentUnit.Skills.GetActiveSkills(),
      PhaseManager.CurrentPhase,
      CurrentUnit
    );
  }

  private void FocusOnUnit() {
    TileManager.ShowReachableTiles(
      CurrentUnit.CurrentTile,
      CurrentUnit.CurrentMovePoints
    );

    _ = CameraController.FocusOn(CurrentUnit.transform.position);
  }

  public bool CheckBattleIsOver() {
    bool hasAllies = Queue.Exists(u =>
      !u.IsDead && u.Relation == UnitRelation.Ally);

    bool hasEnemies = Queue.Exists(u =>
      !u.IsDead && u.Relation == UnitRelation.Enemy);

    if (!hasAllies)
      BattleManager.Instance.battleResult = BattleResult.Defeat;
    else if (!hasEnemies)
      BattleManager.Instance.battleResult = BattleResult.Victory;

    if (BattleManager.Instance.battleResult != null) {
      BattleManager.Instance.Finish();
      return true;
    }
    return false;
  }
}
