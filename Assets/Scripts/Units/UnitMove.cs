using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnitMove : MonoBehaviour {
  private Unit unit;
  private readonly Queue<Tile> path = new();
  public bool IsMoving { get; private set; } = false;
  private readonly int mpToClimb = 2;

  void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit movement components initialization error");
    }
  }

  private void LateUpdate() {
    if (IsMoving && path.Count > 0) MoveAlongPath();
  }

  public async void OnMove(Tile target) {
    List<Tile> pathTiles = Pathfinding.FindPath(
      unit.CurrentTile,
      target,
      unit.CurrentMovePoints
    );

    if (pathTiles == null) return;

    BattleUI.Instance.DisableUI();
    _ = CameraController.FocusOn(target.GetPos());

    unit.CurrentTile.OccupiedBy = null;
    path.Clear();
    foreach (Tile tile in pathTiles) path.Enqueue(tile);
    await Task.Yield();
    IsMoving = true;

    float moveCost = 0f;
    if (unit.CurrentMovePoints == 0) return;
    for (int i = 1; i < pathTiles.Count; i++) {
      moveCost += Pathfinding.GetCost(pathTiles[i - 1], pathTiles[i]);
    }
    unit.CurrentMovePoints -= moveCost;
  }

  private void MoveAlongPath() {
    if (path.Count == 0) return;

    Tile targetTile = path.Peek();
    Vector3 targetPos = targetTile.GetPos();
    Vector3 direction = (targetPos - transform.position).normalized;

    _ = unit.Animator.RotateTowards(direction, true);
    unit.Animator.SetMoving(true);
    transform.position = Vector3.MoveTowards(transform.position, targetPos, unit.MoveSpeed * Time.deltaTime);

    if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
      bool canMove = CheckTileTypeOnMove(targetTile);

      if (canMove) path.Dequeue();
      else path.Clear();

      if (path.Count == 0) {
        unit.CurrentTile = targetTile;
        unit.CurrentTile.OccupiedBy = unit;
        CheckTileTypeOnStop();
        IsMoving = false;
        unit.Animator.SetMoving(false);
        BattleUI.Instance.EnableUI();
        if (canMove) AfterMove();
      }
    }
  }

  private void AfterMove() {
    if (unit.CurrentMovePoints < 1 || unit.Relation == UnitRelation.Enemy) {
      PhaseManager.NextPhase();
    } else {
      TileManager.ShowReachableTiles(
        unit.CurrentTile,
        unit.CurrentMovePoints
      );
    }
  }

  public void Climb() {
    if (unit.CurrentMovePoints < mpToClimb) {
      _ = Toast.Show("warning", "Not enough movement points");
      return;
    }

    Tile tileFrom = unit.CurrentTile;
    if (tileFrom.type != TileType.Climb || tileFrom.climbTo == null) return;
    Tile tileTo = tileFrom.climbTo;

    _ = CameraController.FocusOn(tileTo.GetPos());
    tileFrom.OccupiedBy = null;
    transform.position = tileTo.GetPos();
    unit.CurrentTile = tileTo;
    unit.CurrentTile.OccupiedBy = unit;
    unit.CurrentMovePoints -= mpToClimb;
    AfterMove();
  }

  private bool CheckTileTypeOnMove(Tile tile) {
    switch (tile.type) {
      case TileType.Loot:
        if (unit.Relation == UnitRelation.Ally) tile.TakeLoot();
        return true;
      case TileType.Trap:
        tile.TriggerTrap();
        // FIXME: Проверка на тип ловушки
        return false;
    }
    return true;
  }

  private void CheckTileTypeOnStop() {
    TileType type = unit.CurrentTile.type;

    switch (type) {
      case TileType.Cover:
        Effect coverEffect = Factory.CreateEffectById("e3");
        if (coverEffect == null) break;
        unit.Effects.ApplyEffect(coverEffect);
        break;
      case TileType.Climb:
        if (unit.Relation != UnitRelation.Enemy) BattleUI.Instance.ShowClimbButton();
        break;
    }

    if (type != TileType.Cover) unit.Effects.ClearEffect("Cover");
    if (type != TileType.Climb) BattleUI.Instance.HideClimbButton();
  }
}
