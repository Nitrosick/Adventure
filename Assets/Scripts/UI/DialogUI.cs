using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {
  // private static IconDatabase IconDatabase;
  private static Transform window;
  private static GameObject background;
  private static Button submit;
  private static TextMeshProUGUI submitText;
  private static Button decline;
  private static TextMeshProUGUI declineText;
  private static Action<bool> callback;
  private static Image icon;
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI text;
  private static GameObject effect;
  private static TextMeshProUGUI effectValue;
  private static TextMeshProUGUI warning;

  private void Awake() {
    // IconDatabase = Resources.Load<IconDatabase>("Databases/IconDatabase");
    window = transform.Find("Dialog/Panel").GetComponent<Transform>();
    background = transform.Find("Dialog/Background").gameObject;
    submit = window.Find("Control/Confirm").GetComponent<Button>();
    submitText = window.Find("Control/Confirm/Text").GetComponent<TextMeshProUGUI>();
    decline = window.Find("Control/Decline").GetComponent<Button>();
    declineText = window.Find("Control/Decline/Text").GetComponent<TextMeshProUGUI>();
    icon = window.Find("Head/Icon").GetComponent<Image>();
    title = window.Find("Head/Title").GetComponent<TextMeshProUGUI>();
    text = window.Find("Text").GetComponent<TextMeshProUGUI>();
    effect = window.Find("Effect").gameObject;
    effectValue = window.Find("Effect/Value").GetComponent<TextMeshProUGUI>();
    warning = window.Find("Warning").GetComponent<TextMeshProUGUI>();

    if (
      window == null || background == null || submit == null ||
      decline == null || title == null || text == null ||
      submitText == null || declineText == null || icon == null ||
      effect == null || effectValue == null || warning == null
    ) {
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
    SceneController.OpenWindow("dialog");
  }

  private static void Close() {
    callback = null;
    window.gameObject.SetActive(false);
    submit.gameObject.SetActive(true);
    background.SetActive(false);
    warning.gameObject.SetActive(false);
    title.text = "";
    text.text = "";
    submitText.text = "Yes";
    declineText.text = "No";
    warning.text = "";
    SceneController.CloseWindow("dialog");
  }

  private static void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private static void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public static void Confirmation(
    Action<bool> action,
    string _title = "",
    string _text = "",
    string _warning = ""
  ) {
    callback = action;
    title.text = _title;
    text.text = _text;
    if (_warning != "") {
      warning.gameObject.SetActive(true);
      warning.text = _warning;
    }
    Open();
  }

  public static void Learn(
    Action<bool> action,
    string _title,
    string _text,
    string _effect,
    Sprite _icon,
    int _tier = 0,
    bool active = true,
    string _warning = ""
  ) {
    submit.gameObject.SetActive(active);
    effect.SetActive(_effect != "");
    icon.sprite = _icon;
    submitText.text = "Learn";
    declineText.text = "Close";
    callback = action;
    title.text = _title;
    if (_tier > 0) title.text += $" (tier {_tier})";
    text.text = _text;
    effectValue.text = _effect;
    if (_warning != "") {
      warning.gameObject.SetActive(true);
      warning.text = _warning;
    }
    Open();
  }

  public static void Info(
    string _title = "",
    string _text = "",
    string btnText = ""
  ) {
    submit.gameObject.SetActive(false);
    declineText.text = btnText == "" ? "Ok" : btnText;
    title.text = _title;
    text.text = _text;
    Open();
  }
}
