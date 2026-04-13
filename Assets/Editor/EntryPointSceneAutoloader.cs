using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EntryPointSceneAutoloader {
  private const string MENU_PATH = "Tools/Entry Point Autoloader/Enable";
  private const string PREF_KEY = "EntryPointAutoloader.Enabled";

  static EntryPointSceneAutoloader() {
    ApplySetting();
  }

  [MenuItem(MENU_PATH)]
  private static void Toggle() {
    bool enabled = !EditorPrefs.GetBool(PREF_KEY, false);
    EditorPrefs.SetBool(PREF_KEY, enabled);
    ApplySetting();
  }

  [MenuItem(MENU_PATH, true)]
  private static bool ToggleValidate() {
    Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(PREF_KEY, false));
    return true;
  }

  private static void ApplySetting() {
    bool enabled = EditorPrefs.GetBool(PREF_KEY, false);

    if (!enabled) {
      EditorSceneManager.playModeStartScene = null;
      return;
    }

    if (EditorBuildSettings.scenes.Length == 0) {
      Debug.LogWarning("Build Settings is empty");
      EditorSceneManager.playModeStartScene = null;
      return;
    }

    string path = EditorBuildSettings.scenes[0].path;
    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

    if (sceneAsset == null) {
      Debug.LogWarning("Failed to load entry scene");
      EditorSceneManager.playModeStartScene = null;
      return;
    }

    EditorSceneManager.playModeStartScene = sceneAsset;
  }
}
