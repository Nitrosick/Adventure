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

  private void Awake() {
    panel = transform.Find("PauseMenu/Panel");
    background = transform.Find("PauseMenu/Background").gameObject;
    continueGame = panel.Find("Buttons/Continue").GetComponent<Button>();
    optionsButton = panel.Find("Buttons/Options").GetComponent<Button>();

    if (panel == null || background == null || continueGame == null || optionsButton == null) {
      Debug.LogError("Pause menu components initialization error");
      return;
    }

    Transform mainMenuButtonObj = panel.Find("Buttons/MainMenu");
    if (mainMenuButtonObj != null) {
      mainMenuButton = mainMenuButtonObj.GetComponent<Button>();
      mainMenuButton.onClick.AddListener(ToMainMenu);
    }
    Transform exitGameObj = panel.Find("Buttons/Exit");
    if (exitGameObj != null) {
      exitGame = exitGameObj.GetComponent<Button>();
      exitGame.onClick.AddListener(ExitGame);
    }
    Transform retreatButtonObj = panel.Find("Buttons/Retreat");
    if (retreatButtonObj != null) {
      retreatButton = retreatButtonObj.GetComponent<Button>();
      retreatButton.onClick.AddListener(RetreatConfirmation);
    }

    continueGame.onClick.AddListener(Close);
    // optionsButton.onClick.AddListener(() => {});
  }

  private void OnDestroy() {
    if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(ToMainMenu);
    if (exitGame != null) exitGame.onClick.RemoveListener(ExitGame);
    if (retreatButton != null) retreatButton.onClick.RemoveListener(RetreatConfirmation);
    continueGame.onClick.RemoveListener(Close);
    // optionsButton.onClick.RemoveListener(() => {});
  }

  public static void Open() {
    panel.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.ShowBackground();
  }

  public static void Close() {
    panel.gameObject.SetActive(false);
    background.SetActive(false);
    SceneController.HideBackground();
  }

  private static void ToMainMenu() {
    panel.gameObject.SetActive(false);
    SceneController.SwitchScene("Scenes/Menu");
  }

  private static void ExitGame() {
    Application.Quit();
  }

  private void RetreatConfirmation() {
    Dialog.Confirmation(
      Retreat,
      "Retreating",
      "Do you really want to flee the battlefield?\nIn that case, you will not receive any rewards\nand will lose some fame."
    );
  }

  private static void Retreat(bool accepted) {
    if (!accepted) return;
    Close();
    BattleManager.battleResult = BattleResult.Retreat;
    BattleManager.Finish();
  }
}
