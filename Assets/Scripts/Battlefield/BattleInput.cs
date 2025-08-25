using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BattleInput : MonoBehaviour {
  [SerializeField] private InputActionReference flattenInput;
  private List<Tile> tiles;
  private List<TileFiller> fillers;
  private Transform terrain;
  private Vector3 terrainInitPos;
  private readonly float lerpDuration = 0.25f;

  private void Awake() {
    terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Transform>();
  }

  private void Start() {
    tiles = TileManager.GetHighTiles();
    fillers = TileManager.GetFillers();
    if (terrain == null) return;
    terrainInitPos = terrain.transform.position;
  }

  private void OnEnable() {
    flattenInput.action.performed += OnFlattenPressed;
    flattenInput.action.canceled += OnFlattenReleased;
    flattenInput.action.Enable();
  }

  private void OnDisable() {
    flattenInput.action.performed -= OnFlattenPressed;
    flattenInput.action.canceled -= OnFlattenReleased;
    flattenInput.action.Disable();
  }

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

  private void OnFlattenPressed(InputAction.CallbackContext ctx) {
    if (tiles.Count == 0) return;

    foreach (Tile tile in tiles) {
      float offset = tile.height - (0.1f * tile.height) - 1;
      Vector3 targetPos = tile.InitPosition - new Vector3(0, offset, 0);
      AnimateMove(tile.transform, targetPos);
    }

    foreach (TileFiller filler in fillers) {
      float offset = filler.height - (0.1f * filler.height) - 1;
      Vector3 targetPos = filler.InitPosition - new Vector3(0, offset, 0);
      AnimateMove(filler.transform, targetPos);
    }

    Vector3 terrainTargetPos = terrain.transform.position - new Vector3(0, tiles[0].height - 1, 0);
    AnimateMove(terrain, terrainTargetPos);
  }

  private void OnFlattenReleased(InputAction.CallbackContext ctx) {
    foreach (Tile tile in tiles) {
      AnimateMove(tile.transform, tile.InitPosition);
    }

    foreach (TileFiller filler in fillers) {
      AnimateMove(filler.transform, filler.InitPosition);
    }

    AnimateMove(terrain, terrainInitPos);
  }

  private async void AnimateMove(Transform t, Vector3 target) {
    Vector3 start = t.position;
    float elapsed = 0f;

    while (elapsed < lerpDuration) {
      elapsed += Time.deltaTime;
      float tNorm = Mathf.Clamp01(elapsed / lerpDuration);
      t.position = Vector3.Lerp(start, target, tNorm);
      await Task.Yield();
    }

    t.position = target;
  }
}
