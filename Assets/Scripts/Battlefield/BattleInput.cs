using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BattleInput : MonoBehaviour {
  public static BattleInput Instance;
  [SerializeField] private InputActionReference flattenInput;

  private List<Tile> tiles = new();
  private List<TileFiller> fillers = new();
  private List<GameObject> traps = new();
  private readonly List<Renderer> obstacles = new();
  private readonly Dictionary<Renderer, Color[]> originalColors = new();

  private Transform terrain;
  private Vector3 terrainInitPos;

  private bool isTransparent = false;
  private readonly float lerpDuration = 0.25f;
  private readonly float objectsTransparency = 0.15f;

  void Awake() {
    Instance = this;
    terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Transform>();
    traps = GameObject.FindGameObjectsWithTag("Trap").ToList();

    if (obstacles == null || obstacles.Count == 0) {
      GameObject[] objs = GameObject.FindGameObjectsWithTag("BattlefieldLargeObject");
      foreach (GameObject g in objs) {
        if (g.TryGetComponent<Renderer>(out var r)) obstacles.Add(r);
      }
    }

    CacheOriginalColors();
  }

  void Start() {
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

  void Update() {
    if (
      BattleManager.Instance.battleResult != null ||
      !Mouse.current.leftButton.wasPressedThisFrame
    ) return;

    switch (PhaseManager.CurrentPhase) {
      case BattlePhase.Movement:
        if (!QueueManager.Instance.CurrentUnit.GetComponent<UnitMove>().IsMoving) HandleClick();
        break;
      case BattlePhase.Attack:
        HandleClick();
        break;
    }
  }

  private void CacheOriginalColors() {
    originalColors.Clear();

    foreach (Renderer rend in obstacles.Where(x => x != null)) {
      Material[] mats = rend.materials;
      Color[] cols = new Color[mats.Length];
      for (int i = 0; i < mats.Length; i++) cols[i] = mats[i].color;
      originalColors[rend] = cols;
    }
  }

  private void HandleClick() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    Unit currentUnit = QueueManager.Instance.CurrentUnit;
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
    if (obstacles.Count > 0) SetTransparency(true);
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

    foreach (GameObject trap in traps) {
      if (trap == null) continue;
      Vector3 targetPos = trap.transform.position - new Vector3(0, 1, 0);
      AnimateMove(trap.transform, targetPos);
    }

    Vector3 terrainTargetPos = terrain.transform.position - new Vector3(0, tiles[0].height - 1, 0);
    AnimateMove(terrain, terrainTargetPos);
  }

  private void OnFlattenReleased(InputAction.CallbackContext ctx) {
    if (obstacles.Count > 0) SetTransparency(false);

    foreach (Tile tile in tiles) {
      AnimateMove(tile.transform, tile.InitPosition);
    }

    foreach (TileFiller filler in fillers) {
      AnimateMove(filler.transform, filler.InitPosition);
    }

    foreach (GameObject trap in traps) {
      if (trap == null) continue;
      Vector3 targetPos = trap.transform.position + new Vector3(0, 1, 0);
      AnimateMove(trap.transform, targetPos);
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

  private void SetTransparency(bool on) {
    if (isTransparent == on) return;
    isTransparent = on;

    foreach (Renderer rend in obstacles) {
      if (rend == null) continue;

      if (!originalColors.ContainsKey(rend)) {
        var matsTmp = rend.materials;
        originalColors[rend] = matsTmp.Select(m => m.color).ToArray();
      }

      Material[] mats = rend.materials;
      Color[] originals = originalColors[rend];

      for (int i = 0; i < mats.Length; i++) {
        var mat = mats[i];
        if (mat == null) continue;

        if (on) {
          MakeMaterialTransparent(mat);
          Color col = (i < originals.Length) ? originals[i] : mat.color;
          col.a = objectsTransparency;
          mat.color = col;
        } else {
          if (i < originals.Length) mat.color = originals[i];
          MakeMaterialOpaque(mat);
        }
      }

      rend.materials = mats;
    }
  }

  private static void MakeMaterialTransparent(Material mat) {
    if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
    mat.SetOverrideTag("RenderType", "Transparent");
    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    mat.SetInt("_ZWrite", 0);
    mat.DisableKeyword("_ALPHATEST_ON");
    mat.EnableKeyword("_ALPHABLEND_ON");
    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
  }

  private static void MakeMaterialOpaque(Material mat) {
    if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
    mat.SetOverrideTag("RenderType", "Opaque");
    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
    mat.SetInt("_ZWrite", 1);
    mat.DisableKeyword("_ALPHABLEND_ON");
    mat.renderQueue = -1;
  }
}
