using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : ModalUI {
  public static Dialog Instance;

  private static Button submit;
  private static TextMeshProUGUI submitText;
  private static Button decline;
  private static TextMeshProUGUI declineText;
  private static GameObject effect;
  private static TextMeshProUGUI effectValue;
  private static TextMeshProUGUI warning;

  private void Awake() {
    Instance = this;
  }

  private void Init() {
    window = transform.Find("Modals/Dialog").GetComponent<Transform>();
    base.Init(window);

    submit = window.Find("Control/Confirm").GetComponent<Button>();
    submitText = window.Find("Control/Confirm/Text").GetComponent<TextMeshProUGUI>();
    decline = window.Find("Control/Decline").GetComponent<Button>();
    declineText = window.Find("Control/Decline/Text").GetComponent<TextMeshProUGUI>();
    effect = window.Find("Effect").gameObject;
    effectValue = window.Find("Effect/Value").GetComponent<TextMeshProUGUI>();
    warning = window.Find("Warning").GetComponent<TextMeshProUGUI>();

    if (
      window == null || submit == null || decline == null ||
      submitText == null || declineText == null || effect == null ||
      effectValue == null || warning == null
    ) {
      Debug.LogError("Dialog components initialization error");
      return;
    }

    submit.onClick.AddListener(OnSubmit);
    decline.onClick.AddListener(OnDecline);
  }

  private void OnDestroy() {
    if (submit != null) submit.onClick.RemoveListener(OnSubmit);
    if (decline != null) decline.onClick.RemoveListener(OnDecline);
  }

  protected override void Open() {
    base.Open();
    SceneController.OpenWindow("dialog");
  }

  protected override void Close() {
    base.Close();
    submit.gameObject.SetActive(true);
    warning.gameObject.SetActive(false);
    submitText.text = "Yes";
    declineText.text = "No";
    warning.text = "";
    SceneController.CloseWindow("dialog");
  }

  private void OnSubmit() {
    callback?.Invoke(true);
    Close();
  }

  private void OnDecline() {
    callback?.Invoke(false);
    Close();
  }

  public void Confirmation(
    Action<bool> action,
    string _title = "",
    string _text = "",
    string _warning = ""
  ) {
    Init();
    callback = action;
    title.text = _title;
    text.text = _text;
    if (_warning != "") {
      warning.gameObject.SetActive(true);
      warning.text = _warning;
    }
    Open();
  }

  public void Learn(
    Action<bool> action,
    string _title,
    string _text,
    string _effect,
    Sprite _icon,
    int _tier = 0,
    bool active = true,
    string _warning = ""
  ) {
    Init();
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

  public void Info(
    string _title = "",
    string _text = "",
    string btnText = ""
  ) {
    Init();
    submit.gameObject.SetActive(false);
    declineText.text = btnText == "" ? "Ok" : btnText;
    title.text = _title;
    text.text = _text;
    Open();
  }
}
