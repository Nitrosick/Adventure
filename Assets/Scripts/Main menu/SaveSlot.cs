using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour {
  private Button button;
  private GameObject activePanel;
  private GameObject emptyPanel;
  private TextMeshProUGUI saveName;
  private TextMeshProUGUI saveDate;

  public int index;
  private bool hasSave;

  private void Awake() {
    button = transform.GetComponent<Button>();
    activePanel = transform.Find("Active").gameObject;
    emptyPanel = transform.Find("Empty").gameObject;
    saveName = transform.Find("Active/Name").GetComponent<TextMeshProUGUI>();
    saveDate = transform.Find("Active/Date").GetComponent<TextMeshProUGUI>();

    if (button == null || activePanel == null || emptyPanel == null || saveName == null || saveDate == null) {
      Debug.LogError("Save slot components initialization error");
    }
  }

  public void Init(SaveData data) {
    if (data != null) {
      hasSave = true;
      saveName.text = data.saveName;
      saveDate.text = data.saveTime;
      emptyPanel.SetActive(false);
      activePanel.SetActive(true);
    } else {
      hasSave = false;
    }

    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(OnClick);
  }

  public void OnClick() {
    if (hasSave) StateManager.LoadGame(index);
    else  InitNewGame();
    SceneController.SwitchScene("Scenes/Map/Village");
  }

  private void InitNewGame() {
    SaveData newSave = new() {
      saveName = "New game",
      saveTime = DateTime.Now.ToString()
      // ...
    };

    StateManager.SaveGame(newSave, index);
    Init(newSave);
  }
}
