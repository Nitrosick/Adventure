using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
  private Transform main;
  private CanvasGroup savesPanel;
  private Transform slotsContainer;
  private Button startGame;
  private Button exitGame;

  public GameObject saveSlot;
  private SaveSlot[] activeSlots;

  private void Awake() {
    main = transform.Find("Menu");
    savesPanel = transform.Find("SaveSlots").GetComponent<CanvasGroup>();
    slotsContainer = transform.Find("SaveSlots/List");
    startGame = main.Find("Start").GetComponent<Button>();
    exitGame = main.Find("Exit").GetComponent<Button>();
    activeSlots = slotsContainer.GetComponentsInChildren<SaveSlot>(includeInactive: false);

    if (
      main == null || savesPanel == null || slotsContainer == null ||
      startGame == null || exitGame == null || activeSlots == null ||
      activeSlots.Length < 5
    ) {
      Debug.LogError("Main menu components initialization error");
      return;
    }

    startGame.onClick.AddListener(OpenSaveSlots);
    exitGame.onClick.AddListener(ExitGame);

    foreach (SaveSlot slot in activeSlots) {
      var data = StateManager.SaveExists(slot.index)
        ? StateManager.LoadGame(slot.index)
        : null;
      slot.Init(data);
    }
  }

  private void OnDestroy() {
    startGame.onClick.RemoveListener(OpenSaveSlots);
    exitGame.onClick.RemoveListener(ExitGame);
  }

  private void OpenSaveSlots() {
    savesPanel.alpha = 1f;
  }

  private void ExitGame() {
    Application.Quit();
  }
}
