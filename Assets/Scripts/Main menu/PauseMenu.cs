using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
  private static Transform panel;
  private static GameObject background;
  private static Button continueGame;
  private static Button optionsButton;
  private static Button mainMenuButton;
  private static Button exitGame;
  private static Button retreatButton;

  void Awake() {
    panel = transform.Find("Menu/Pause");
    background = transform.Find("Menu/Background").gameObject;
    continueGame = panel.Find("Buttons/Continue").GetComponent<Button>();
    optionsButton = panel.Find("Buttons/Options").GetComponent<Button>();

    if (panel == null || background == null || continueGame == null || optionsButton == null) {
      Debug.LogError("Pause menu components initialization error");
      return;
    }

    Transform mainMenuButtonObj = panel.Find("Buttons/MainMenu");
    if (mainMenuButtonObj != null) {
      mainMenuButton = mainMenuButtonObj.GetComponent<Button>();
      mainMenuButton.onClick.AddListener(ToMainMenuConfirmation);
    }
    Transform exitGameObj = panel.Find("Buttons/Exit");
    if (exitGameObj != null) {
      exitGame = exitGameObj.GetComponent<Button>();
      exitGame.onClick.AddListener(ExitConfirmation);
    }
    Transform retreatButtonObj = panel.Find("Buttons/Retreat");
    if (retreatButtonObj != null) {
      retreatButton = retreatButtonObj.GetComponent<Button>();
      retreatButton.onClick.AddListener(RetreatConfirmation);
    }

    continueGame.onClick.AddListener(Close);
    // optionsButton.onClick.AddListener(() => {});
  }

  void OnDestroy() {
    if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(ToMainMenuConfirmation);
    if (exitGame != null) exitGame.onClick.RemoveListener(ExitConfirmation);
    if (retreatButton != null) retreatButton.onClick.RemoveListener(RetreatConfirmation);
    continueGame.onClick.RemoveListener(Close);
    // optionsButton.onClick.RemoveListener(() => {});
  }

  public static void Open() {
    panel.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.OpenWindow("pause");
  }

  public static void Close() {
    panel.gameObject.SetActive(false);
    background.SetActive(false);
    SceneController.CloseWindow("pause");
  }

  private static void ToMainMenuConfirmation() {
    string text = ExitText();

    Dialog.Instance.Confirmation(
      ToMainMenu,
      "Exit the game",
      text
    );
  }

  private static void ToMainMenu(bool accepted) {
    if (!accepted) return;
    panel.gameObject.SetActive(false);
    SceneController.SwitchScene("Scenes/Menu");
  }

  private static void ExitConfirmation() {
    string text = ExitText();

    Dialog.Instance.Confirmation(
      ExitGame,
      "Exit the game",
      text
    );
  }

  private static void ExitGame(bool accepted) {
    if (!accepted) return;
    Application.Quit();
  }

  private static void RetreatConfirmation() {
    Dialog.Instance.Confirmation(
      Retreat,
      "Retreating",
      "Do you really want to flee the battlefield?\nIn that case, you will not receive any rewards\nand will lose some fame."
    );
  }

  private static void Retreat(bool accepted) {
    if (!accepted) return;
    Close();
    BattleManager.Instance.battleResult = BattleResult.Retreat;
    BattleManager.Instance.Finish();
  }

  private static string ExitText() {
    SaveData data = StateManager.LoadGame(StateManager.saveSlot, false);
    string lastSave = data.saveTime ?? "-";    
    return $"Are you sure you want to quit the game?\nYou will lose unsaved progress.\nLast save: <b>{lastSave}</b>";
  }
}
