using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {
  private static Transform window;
  private static GameObject background;
  private static Button submit;
  private static Button decline;
  private static Action<bool> callback;
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI text;

  private void Awake() {
    window = transform.Find("Dialog/Panel").GetComponent<Transform>();
    background = transform.Find("Dialog/Background").gameObject;
    submit = window.Find("Control/Confirm").GetComponent<Button>();
    decline = window.Find("Control/Decline").GetComponent<Button>();
    title = window.Find("Head/Title").GetComponent<TextMeshProUGUI>();
    text = window.Find("Text").GetComponent<TextMeshProUGUI>();

    if (window == null || background == null || submit == null || decline == null || title == null || text == null) {
      Debug.LogError("Dialog components initialization error");
      return;
    }

    submit.onClick.AddListener(OnSubmit);
    decline.onClick.AddListener(OnDecline);
  }

  private void OnDestroy() {
    submit.onClick.RemoveListener(OnSubmit);
    decline.onClick.RemoveListener(OnDecline);
  }

  private static void Open() {
    window.gameObject.SetActive(true);
    background.SetActive(true);
    SceneController.ShowBackground();
  }

  private static void Close() {
    callback = null;
    window.gameObject.SetActive(false);
    background.SetActive(false);
    SceneController.HideBackground();
    title.text = "";
    text.text = "";
  }

  private static void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private static void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public static void Confirmation(Action<bool> action, string _title = "", string _text = "") {
    callback = action;
    title.text = _title;
    text.text = _text;
    Open();
  }
}
