using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalUI : MonoBehaviour {
  protected Transform window;
  protected GameObject background;
  protected Action<bool> callback;
  protected Image icon;
  protected TextMeshProUGUI title;
  protected TextMeshProUGUI text;

  protected virtual void Init(Transform _window) {
    window = _window;
    background = transform.Find("Modals/Background").gameObject;
    icon = window.Find("Head/Icon").GetComponent<Image>();
    title = window.Find("Head/Title").GetComponent<TextMeshProUGUI>();
    text = window.Find("Text").GetComponent<TextMeshProUGUI>();

    if (background == null || title == null || text == null || icon == null) {
      Debug.LogError("Modal components initialization error");
    }
  }

  protected virtual void Open() {
    window.gameObject.SetActive(true);
    background.SetActive(true);
  }

  protected virtual void Close() {
    callback = null;
    window.gameObject.SetActive(false);
    background.SetActive(false);
    title.text = "";
    text.text = "";
  }
}
