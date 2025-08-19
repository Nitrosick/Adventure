using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BattleInput : MonoBehaviour {
  private void Update() {
    if (
      BattleManager.battleResult != null ||
      !Mouse.current.leftButton.wasPressedThisFrame
    ) return;

    switch (PhaseManager.CurrentPhase) {
      case BattlePhase.Movement:
        if (!QueueManager.CurrentUnit.GetComponent<UnitMove>().IsMoving) HandleClick();
        break;
      case BattlePhase.Attack:
        HandleClick();
        break;
    }
  }

  private void HandleClick() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    Unit currentUnit = QueueManager.CurrentUnit;
    if (currentUnit == null) return;

    if (Physics.Raycast(ray, out RaycastHit hit)) {
      string tag = hit.collider.gameObject.tag;

      switch (tag) {
        case "Unit":
          if (hit.collider.TryGetComponent<Unit>(out var clickedUnit)) {
            if (
              currentUnit == clickedUnit ||
              currentUnit.Relation == clickedUnit.Relation ||
              !clickedUnit.CurrentTile.AttackGrid.activeSelf
            ) return;

            TileManager.HideGrid();
            currentUnit.OnAttack(clickedUnit);
          }
          break;

        case "BattlefieldTile":
          if (hit.collider.TryGetComponent<Tile>(out var clickedTile)) {
            if (
              clickedTile == null ||
              !TileManager.TileIsWalkable(currentUnit.CurrentTile, clickedTile) ||
              !clickedTile.Grid.activeSelf
            ) return;

            TileManager.HideGrid();
            currentUnit.GetComponent<UnitMove>().OnMove(clickedTile);
          }
          break;

        case "Breakable":
          if (PhaseManager.CurrentPhase != BattlePhase.Attack || !currentUnit.Equip.CanBreakObjects()) return;

          if (hit.collider.TryGetComponent<Breakable>(out var breakable)) {
            if (breakable == null || !breakable.ParentTile.AttackGrid.activeSelf) return;
            TileManager.HideGrid();
            currentUnit.BreakObject(breakable);
          }
          break;

        case "Tree":
          if (PhaseManager.CurrentPhase != BattlePhase.Attack || !currentUnit.Equip.CanChopTrees()) return;

          if (hit.collider.TryGetComponent<TreeObject>(out var chopable)) {
            if (chopable == null || !chopable.ParentTile.AttackGrid.activeSelf) return;
            TileManager.HideGrid();
            currentUnit.ChopTree(chopable);
          }
          break;
      }
    }
  }
}
