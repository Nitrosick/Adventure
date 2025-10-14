using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneSwitcher
{
  [MenuItem("Scenes/Open Main Menu Scene %#&0")] // Ctrl+Shift+Alt+0
  public static void OpenMainMenuScene() {
    EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
  }

  [MenuItem("Scenes/Open Map Scene %#&-")] // Ctrl+Shift+Alt+-
  public static void OpenMapScene() {
    EditorSceneManager.OpenScene("Assets/Scenes/Map/Dunpine village.unity");
  }

  [MenuItem("Scenes/Open Battle Scene 1 %#&1")] // Ctrl+Shift+Alt+1
  public static void OpenSceneBF1() {
    EditorSceneManager.OpenScene("Assets/Scenes/Battlefield/bf1.unity");
  }

  [MenuItem("Scenes/Open Last Battle Scene %#&2")] // Ctrl+Shift+Alt+2
  public static void OpenSceneBF6() {
    EditorSceneManager.OpenScene("Assets/Scenes/Battlefield/bf6.unity");
  }
}
