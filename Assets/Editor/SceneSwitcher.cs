using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneSwitcher
{
  [MenuItem("Scenes/Open Main Menu Scene %#&1")] // Ctrl+Shift+Alt+1
  public static void OpenMainMenuScene() {
    EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
  }

  [MenuItem("Scenes/Open Map Scene %#&2")] // Ctrl+Shift+Alt+2
  public static void OpenMapScene() {
    EditorSceneManager.OpenScene("Assets/Scenes/Map/Dunpine village.unity");
  }

  [MenuItem("Scenes/Open Battle Scene %#&3")] // Ctrl+Shift+Alt+3
  public static void OpenBattleScene() {
    EditorSceneManager.OpenScene("Assets/Scenes/Battlefield/1.unity");
  }
}
