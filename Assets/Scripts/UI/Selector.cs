using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Selector : MonoBehaviour {
  public static Selector Instance;
  public GameObject itemPrefab;

  private static Transform window;
  private static GameObject background;
  private static GameObject placeholder;
  private static GameObject list;
  private static Button cancel;
  private static TextMeshProUGUI title;

  private void Awake() {
    Instance = this;
    window = transform.Find("Selector/Panel").GetComponent<Transform>();
    background = transform.Find("Selector/Background").gameObject;
    placeholder = window.Find("Empty").gameObject;
    list = window.Find("List").gameObject;
    cancel = window.Find("Cancel").GetComponent<Button>();
    title = window.Find("Title").GetComponent<TextMeshProUGUI>();

    if (window == null || background == null || cancel == null || title == null || placeholder == null || list == null) {
      Debug.LogError("Selector components initialization error");
      return;
    }

    cancel.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    cancel.onClick.RemoveListener(Close);
  }

  private static void Open() {
    window.gameObject.SetActive(true);
    background.SetActive(true);
  }

  public static void Close() {
    foreach (Transform child in list.transform) Destroy(child.gameObject);
    window.gameObject.SetActive(false);
    background.SetActive(false);
    placeholder.SetActive(false);
    list.SetActive(false);
    title.text = "";
  }

  public static void List(Action<object> action, List<Equipment> items, string _title = "") {
    if (items.Count == 0) {
      placeholder.SetActive(true);
    } else {
      list.SetActive(true);
      foreach (Equipment item in items) {
        GameObject obj = Instantiate(Instance.itemPrefab, list.transform);
        obj.GetComponent<SelectorItem>().Init(item, action);
      }
    }
    title.text = _title;
    Open();
  }
}
