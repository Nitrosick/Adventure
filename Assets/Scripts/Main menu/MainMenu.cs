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
    main = transform.Find("Menu/Panel");
    savesPanel = transform.Find("SaveSlots/Panel").GetComponent<CanvasGroup>();
    slotsContainer = transform.Find("SaveSlots/Panel/List");
    startGame = main.Find("Buttons/Start").GetComponent<Button>();
    exitGame = main.Find("Buttons/Exit").GetComponent<Button>();
    activeSlots = slotsContainer.GetComponentsInChildren<SaveSlot>();

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
    StateManager.ResetPlayerData();
  }

  private void Start() {
    foreach (SaveSlot slot in activeSlots) {
      var data = StateManager.SaveExists(slot.index)
        ? StateManager.LoadGame(slot.index, false)
        : null;
      slot.Init(data);
    }
  }

  private void OnDestroy() {
    activeSlots = new SaveSlot[] { };
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
