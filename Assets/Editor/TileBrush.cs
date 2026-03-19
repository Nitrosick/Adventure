using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomGridBrush(true, false, false, "Tile Brush")]
[CreateAssetMenu(fileName = "TileBrush", menuName = "Brushes/Tile Brush")]
public class TileBrush : GridBrushBase {
  public enum TileRotation { Random, Left, Top, Right, Bottom }

  [HideInInspector] public TileRotation tileRotation;
  [HideInInspector, Range(-2, 2)] public int tileOffset = 0;
  [HideInInspector] public int selectedCategoryIndex = 0;
  [HideInInspector] public int selectedSetIndex = 0;

  public readonly List<TileBrushSet> availableSets = new();
  public readonly List<string> categories = new();
  public readonly Dictionary<string, List<TileBrushSet>> setsByCategory = new();

  private List<GameObject> currentPrefabs = new();

  public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position) {
    if (currentPrefabs == null || currentPrefabs.Count == 0) return;

    GameObject prefab = currentPrefabs[Random.Range(0, currentPrefabs.Count)];
    if (prefab == null) return;

    Vector3 worldPos = grid.CellToWorld(position);
    worldPos.x += 0.5f;
    worldPos.y += tileOffset;
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

#if UNITY_EDITOR
  [CustomEditor(typeof(TileBrush))]
  public class TileBrushInspector : Editor {
    private void OnEnable() {
      SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable() {
      SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView) {
      Event e = Event.current;
      TileBrush brush = (TileBrush)target;

      if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R) {
        brush.tileRotation =
          (TileRotation)(((int)brush.tileRotation + 1) %
          System.Enum.GetValues(typeof(TileRotation)).Length);

        e.Use();
        EditorUtility.SetDirty(brush);
      }
    }

    public override void OnInspectorGUI() {
      TileBrush brush = (TileBrush)target;

      LoadAvailableSets();

      if (brush.categories.Count > 0) {
        brush.selectedCategoryIndex = EditorGUILayout.Popup(
          "Category",
          brush.selectedCategoryIndex,
          brush.categories.ToArray()
        );

        string category = brush.categories[brush.selectedCategoryIndex];
        var sets = brush.setsByCategory[category];
        string[] setNames = new string[sets.Count];

        for (int i = 0; i < sets.Count; i++) setNames[i] = sets[i].brushSetName;

        brush.selectedSetIndex = EditorGUILayout.Popup(
          "Brush Set",
          brush.selectedSetIndex,
          setNames
        );
      } else {
        EditorGUILayout.LabelField("No Brush Sets found");
      }

      brush.tileRotation = (TileRotation)EditorGUILayout.EnumPopup("Rotation", brush.tileRotation);
      brush.tileOffset = EditorGUILayout.IntSlider("Offset", brush.tileOffset, -2, 2);

      if (GUI.changed) {
        UpdateCurrentSet();
        EditorUtility.SetDirty(brush);
      }
    }

    public void LoadAvailableSets() {
      TileBrush brush = (TileBrush)target;

      brush.categories.Clear();
      brush.setsByCategory.Clear();

      string root = "Assets/Editor/Brushes";
      string[] guids = AssetDatabase.FindAssets("t:TileBrushSet", new[] { root });

      foreach (string guid in guids) {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        TileBrushSet set = AssetDatabase.LoadAssetAtPath<TileBrushSet>(path);
        if (set == null) continue;

        string folderPath = System.IO.Path.GetDirectoryName(path);
        string folder = System.IO.Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(folder)) folder = "Default";

        if (!brush.setsByCategory.ContainsKey(folder))
          brush.setsByCategory[folder] = new List<TileBrushSet>();

        brush.setsByCategory[folder].Add(set);
      }

      brush.categories.AddRange(brush.setsByCategory.Keys);
      brush.categories.Sort();

      if (brush.selectedCategoryIndex >= brush.categories.Count)
        brush.selectedCategoryIndex = 0;

      UpdateCurrentSet();
    }

    public void UpdateCurrentSet() {
      TileBrush brush = (TileBrush)target;

      if (brush.categories.Count == 0) return;

      string category = brush.categories[brush.selectedCategoryIndex];
      var sets = brush.setsByCategory[category];

      if (brush.selectedSetIndex >= sets.Count)
        brush.selectedSetIndex = 0;

      brush.currentPrefabs = sets.Count > 0
        ? sets[brush.selectedSetIndex].prefabs
        : new List<GameObject>();
    }

    public void ApplySelectedSet() {
      TileBrush brush = (TileBrush)target;

      brush.currentPrefabs = (brush.selectedSetIndex >= 0 && brush.selectedSetIndex < brush.availableSets.Count)
        ? brush.availableSets[brush.selectedSetIndex].prefabs
        : new List<GameObject>();
    }
#endif
  }
}
