using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomGridBrush(true, false, false, "Tile Brush")]
[CreateAssetMenu(fileName = "TileBrush", menuName = "Brushes/Tile Brush")]
public class TileBrush : GridBrushBase {
  public enum TileRotation { Random, Left, Top, Right, Bottom }

  [SerializeField, HideInInspector] private int selectedSetIndex = 0;
  [SerializeField, HideInInspector] public TileRotation tileRotation;

  private List<TileBrushSet> availableSets = new();
  private List<GameObject> currentPrefabs = new();

#if UNITY_EDITOR
  [CustomEditor(typeof(TileBrush))]
  public class TileBrushInspector : Editor {
    public override void OnInspectorGUI() {
      TileBrush brush = (TileBrush)target;

      brush.LoadAvailableSets();

      string[] names = new string[brush.availableSets.Count];
      for (int i = 0; i < names.Length; i++) {
        names[i] = brush.availableSets[i]?.brushSetName ?? $"Set {i}";
      }

      brush.selectedSetIndex = EditorGUILayout.Popup("Brush Set", brush.selectedSetIndex, names);
      brush.tileRotation = (TileRotation)EditorGUILayout.EnumPopup("Rotation", brush.tileRotation);

      if (GUI.changed) {
        brush.ApplySelectedSet();
        EditorUtility.SetDirty(brush);
      }
    }
  }

  public void LoadAvailableSets() {
    availableSets.Clear();
    string[] guids = AssetDatabase.FindAssets("t:TileBrushSet", new[] { "Assets/Editor/Brushes" });

    foreach (string guid in guids) {
      string path = AssetDatabase.GUIDToAssetPath(guid);
      TileBrushSet set = AssetDatabase.LoadAssetAtPath<TileBrushSet>(path);
      if (set != null) availableSets.Add(set);
    }

    ApplySelectedSet();
  }

  public void ApplySelectedSet() {
    currentPrefabs = (selectedSetIndex >= 0 && selectedSetIndex < availableSets.Count)
        ? availableSets[selectedSetIndex].prefabs
        : new List<GameObject>();
  }
#endif

  public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position) {
#if UNITY_EDITOR
    if (currentPrefabs == null || currentPrefabs.Count == 0) LoadAvailableSets();
#endif
    if (currentPrefabs == null || currentPrefabs.Count == 0) return;

    GameObject prefab = currentPrefabs[Random.Range(0, currentPrefabs.Count)];
    if (prefab == null) return;

    Vector3 worldPos = grid.CellToWorld(position);
    worldPos.x += 0.5f;
    worldPos.z += 0.5f;

    foreach (Transform child in brushTarget.transform) {
      if (grid.WorldToCell(child.position) == position) {
        Undo.DestroyObjectImmediate(child.gameObject);
        break;
      }
    }

    int yAngle = 0;
    switch (tileRotation) {
      case TileRotation.Random:
        int[] angles = { 0, 90, 180, 270 };
        yAngle = angles[Random.Range(0, angles.Length)];
        break;
      case TileRotation.Top: yAngle = 90; break;
      case TileRotation.Right: yAngle = 180; break;
      case TileRotation.Bottom: yAngle = 270; break;
    }

    Quaternion rotation = Quaternion.Euler(0f, yAngle, 0f);
    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    Undo.RegisterCreatedObjectUndo(instance, "Paint Random Prefab");
    instance.transform.SetParent(brushTarget.transform);
    instance.transform.SetPositionAndRotation(worldPos, rotation);
    instance.transform.localScale = Vector3.one * 0.5f;
  }

  public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position) {
    if (brushTarget == null) return;

    foreach (Transform child in brushTarget.transform) {
      if (grid.WorldToCell(child.position) == position) {
        Undo.DestroyObjectImmediate(child.gameObject);
        break;
      }
    }
  }
}
