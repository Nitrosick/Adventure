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

  [MenuItem("Scenes/Open previous Battle scene %#&2")] // Ctrl+Shift+Alt+2
  public static void OpenSceneBF9() {
    EditorSceneManager.OpenScene("Assets/Scenes/Battlefield/bf9.unity");
  }

  [MenuItem("Scenes/Open Last Battle Scene %#&1")] // Ctrl+Shift+Alt+1
  public static void OpenSceneBF11() {
    EditorSceneManager.OpenScene("Assets/Scenes/Battlefield/bf11.unity");
  }
}
