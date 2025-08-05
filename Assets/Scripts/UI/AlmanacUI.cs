using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanacUI : MonoBehaviour {
  // Components
  public static AlmanacUI Instance;
  private static Transform menu;
  private static Transform content;
  public GameObject articleButtonPrefab;

  // Navigation
  private static Transform navigation;
  private static Button closeButton;

  //Sections
  // private static Transform welcomeSection;
  // private static HealingMenuUI healingSection;
  // private static TradingMenuUI tradingSection;
  // private static CraftingMenuUI craftingSection;

  private void Awake() {
    Instance = this;
    menu = transform.Find("Almanac/Panel");
    content = menu.Find("Content/Viewport/Scroll");

    static Transform Find(string path) => menu.Find(path);
    // static Transform FindInContent(string path) => content.Find(path);
    static T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();
    // T GetInContent<T>(string path) where T : Component => content.Find(path).GetComponent<T>();

    navigation = Find("Navigation/Articles");
    closeButton = Get<Button>("Navigation/Control/Close");

    if (!ComponentsInitialized()) {
      Debug.LogError("Almanac components initialization error");
      return;
    }

    closeButton.onClick.AddListener(Close);
  }

  private static bool ComponentsInitialized() {
    return menu != null && navigation != null && closeButton != null;
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
  }

  private static void Init() {
    if (Instance.articleButtonPrefab == null) return;
    List<KnowledgeArticle> articles = KnowledgeManager.GetUnlockedArticles();

    foreach (KnowledgeArticle a in articles) {
      GameObject btn = Instantiate(Instance.articleButtonPrefab, navigation);
      btn.transform.Find("Icon").GetComponent<Image>().sprite = a.icon;
      btn.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = a.title;

      Button btnScript = btn.transform.GetComponent<Button>();
      btnScript.onClick.RemoveAllListeners();
      btnScript.onClick.AddListener(() => ShowContent(a));
    }
  }

  private static void ShowContent(KnowledgeArticle article) {
    Debug.Log(article);
  }
}
