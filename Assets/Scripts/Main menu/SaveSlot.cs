using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour {
  private Button button;
  private GameObject activePanel;
  private GameObject emptyPanel;
  private TextMeshProUGUI sceneName;
  private TextMeshProUGUI hero;
  private TextMeshProUGUI saveDate;
  private Button deleteButton;

  public int index;
  private bool hasSave;

  private void Awake() {
    button = transform.GetComponent<Button>();
    activePanel = transform.Find("Active").gameObject;
    emptyPanel = transform.Find("Empty").gameObject;
    sceneName = transform.Find("Active/Scene").GetComponent<TextMeshProUGUI>();
    hero = transform.Find("Active/Hero").GetComponent<TextMeshProUGUI>();
    saveDate = transform.Find("Active/Date").GetComponent<TextMeshProUGUI>();
    deleteButton = transform.Find("Delete").GetComponent<Button>();

    if (
      button == null || activePanel == null || emptyPanel == null ||
      sceneName == null || saveDate == null || deleteButton == null ||
      hero == null
    ) {
      Debug.LogError("Save slot components initialization error");
    }
  }

  public void Init(SaveData data) {
    if (data != null) {
      hasSave = true;
      sceneName.text = data.currentScene;
      hero.text = $"Warrior | Level {data.level}";
      saveDate.text = data.saveTime;
      emptyPanel.SetActive(false);
      activePanel.SetActive(true);
      deleteButton.gameObject.SetActive(true);

      deleteButton.onClick.RemoveAllListeners();
      deleteButton.onClick.AddListener(DeleteConfirmation);
    } else {
      hasSave = false;
    }

    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(OnClick);
  }

  public void OnClick() {
    StateManager.saveSlot = index;
    if (hasSave) StateManager.LoadGame(index);
    else InitNewGame();
    SceneController.SwitchScene("Scenes/Map/Dunpine village");
  }

  private void InitNewGame() {
    StateManager.InitPlayerArmy();
    StateManager.SaveGame();
    Init(StateManager.GetSaveData());
  }

  private void DeleteConfirmation() {
    Dialog.Confirmation(
      DeleteSlot,
      "Save slot deleting",
      "Do you really want to delete this save slot?\nAll progress will be lost and cannot be recovered."
    );
  }

  private void DeleteSlot(bool accepted) {
    if (!accepted) return;
    StateManager.DeleteSave(index);
    _ = Toast.Show("info", "Save deleted");

    hasSave = false;
    sceneName.text = "";
    hero.text = "";
    saveDate.text = "";
    emptyPanel.SetActive(true);
    activePanel.SetActive(false);
    deleteButton.gameObject.SetActive(false);
  }
}
