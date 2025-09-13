using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanacUI : MonoBehaviour {
  // Components
  public static AlmanacUI Instance;
  public GameObject articleButtonPrefab;

  private static Transform menu;
  private static Transform content;
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI text;

  // Navigation
  private static Transform navigation;
  private static Button closeButton;
  private static readonly Dictionary<string, GameObject> buttons = new() { };

  private void Awake() {
    Instance = this;
    menu = transform.Find("Almanac/Panel");
    content = menu.Find("Content/Viewport/Content");

    static Transform Find(string path) => menu.Find(path);
    static T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();
    static T GetInContent<T>(string path) where T : Component => content.Find(path).GetComponent<T>();

    navigation = Find("Navigation/Articles");
    closeButton = Get<Button>("Navigation/Control/Close");
    title = GetInContent<TextMeshProUGUI>("Title");
    text = GetInContent<TextMeshProUGUI>("Text");

    if (!ComponentsInitialized()) {
      Debug.LogError("Almanac components initialization error");
      return;
    }

    closeButton.onClick.AddListener(Close);
  }

  private static bool ComponentsInitialized() {
    return menu != null && navigation != null && closeButton != null &&
    title != null && text != null;
  }

  private void OnDestroy() {
    closeButton.onClick.RemoveListener(Close);
  }

  public static void Open() {
    menu.gameObject.SetActive(true);
    Clear();
    Init();
    SceneController.OpenWindow("almanac");
  }

  public static void Close() {
    menu.gameObject.SetActive(false);
    Clear();
    SceneController.CloseWindow("almanac");
  }

  private static void Clear() {
    foreach (Transform child in navigation) Destroy(child.gameObject);
    title.text = "";
    text.text = "";
    buttons.Clear();
  }

  private static void Init() {
    if (Instance.articleButtonPrefab == null) return;

    foreach (KnowledgeInstance a in KnowledgeManager.articles) {
      GameObject btn = Instantiate(Instance.articleButtonPrefab, navigation);
      btn.transform.Find("Icon").GetComponent<Image>().sprite = a.data.icon;
      btn.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = a.data.title;

      GameObject newMark = btn.transform.Find("New").gameObject;
      buttons[a.data.id] = newMark;
      if (a.isNew) newMark.SetActive(true);

      Button btnScript = btn.transform.GetComponent<Button>();
      btnScript.onClick.RemoveAllListeners();
      btnScript.onClick.AddListener(() => ShowContent(a));
    }

    ShowContent(KnowledgeManager.articles[0]);
  }

  private static void ShowContent(KnowledgeInstance article) {
    title.text = article.data.title;
    text.text = article.data.content;

    if (article.isNew) {
      article.isNew = false;
      buttons[article.data.id].SetActive(false);
      MapUI.Instance.UpdateAlmanacIcon();
    }
  }
}
