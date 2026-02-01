using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : ModalUI {
  public static Dialog Instance;

  private Button submit;
  private TextMeshProUGUI submitText;
  private Button decline;
  private TextMeshProUGUI declineText;
  private GameObject effect;
  private TextMeshProUGUI effectValue;
  private TextMeshProUGUI warning;

  void Awake() {
    Instance = this;
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

  void OnDestroy() {
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
    submit.gameObject.SetActive(false);
    declineText.text = btnText == "" ? "Ok" : btnText;
    title.text = _title;
    text.text = _text;
    Open();
  }
}
