using System.Collections.Generic;
using UnityEngine;

public class UnitMove : MonoBehaviour {
  private Unit unit;
  private readonly Queue<Vector3> path = new();

  public bool IsMoving { get; private set; } = false;

  private void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit movement components initialization error");
    }
  }

  private void Update() {
    if (IsMoving && path.Count > 0) MoveAlongPath();
  }

  public void OnMove(Tile target) {
    List<Tile> pathTiles = Pathfinding.FindPath(
      unit.CurrentTile,
      target,
      unit.CurrentMovePoints
    );

    if (pathTiles == null) return;

    BattleUI.DisableUI();
    _ = CameraController.FocusOn(target.transform.position);

    unit.CurrentTile.OccupiedBy = null;
    path.Clear();

    foreach (Tile tile in pathTiles) {
      Vector3 center = new(tile.Coords.x + 0.5f, tile.height, tile.Coords.y + 0.5f);
      path.Enqueue(center);
    }

    IsMoving = true;
    unit.CurrentTile = target;
    unit.CurrentTile.OccupiedBy = unit;

    float moveCost = 0f;
    for (int i = 1; i < pathTiles.Count; i++) {
      moveCost += Pathfinding.GetCost(pathTiles[i - 1], pathTiles[i]);
    }
    unit.CurrentMovePoints -= moveCost;
  }

  private void MoveAlongPath() {
    if (path.Count == 0) return;

    Vector3 target = path.Peek();
    Vector3 direction = (target - transform.position).normalized;

    _ = unit.Animator.RotateTowards(direction, true);
    unit.Animator.SetMoving(true);
    transform.position = Vector3.MoveTowards(transform.position, target, unit.MoveSpeed * Time.deltaTime);

    if (Vector3.Distance(transform.position, target) < 0.01f) {
      path.Dequeue();

      if (path.Count == 0) {
        CheckTileType();
        IsMoving = false;
        unit.Animator.SetMoving(false);
        BattleUI.EnableUI();

        if (unit.CurrentMovePoints < 1 || unit.Relation == UnitRelation.Emeny) {
          PhaseManager.NextPhase();
        }
        else {
          TileManager.ShowReachableTiles(
            unit.CurrentTile,
            unit.CurrentMovePoints
          );
        }
      }
    }
  }

  private void CheckTileType() {
    if (unit.CurrentTile.type == TileType.Cover) {
      Effect coverEffect = Resources.Load<Effect>("Effects/Cover");
      unit.Effects.ApplyEffect(coverEffect);
    } else {
      unit.Effects.ClearEffect("Cover");
    }

    if (unit.CurrentTile.type == TileType.Loot && unit.Relation == UnitRelation.Ally) {
      unit.CurrentTile.TakeLoot();
    }
  }
}
