using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanacUI : MonoBehaviour {
  // Components
  public static AlmanacUI Instance;
  private static IconDatabase IconDatabase;
  public GameObject sectionPrefab;
  public GameObject articlePrefab;

  private Transform menu;
  private Transform content;
  private TextMeshProUGUI title;
  private TextMeshProUGUI text;

  // Navigation
  private Transform navigation;
  private Button closeButton;
  private Dictionary<KnowledgeSection, List<KnowledgeInstance>> sections = new() { };

  private readonly Dictionary<KnowledgeSection, string> sectionIcons = new() {
    { KnowledgeSection.Common, "info" },
    { KnowledgeSection.AdventureMap, "map" },
    { KnowledgeSection.Battlefield, "battle" },
    { KnowledgeSection.Lore, "learn" },
    { KnowledgeSection.Player, "player" }
  };

  private void Awake() {
    Instance = this;
    IconDatabase = Resources.Load<IconDatabase>("Databases/IconDatabase");
    menu = transform.Find("Almanac/Panel");
    content = menu.Find("Content/Viewport/Content");

     Transform Find(string path) => menu.Find(path);
     T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();
     T GetInContent<T>(string path) where T : Component => content.Find(path).GetComponent<T>();

    navigation = Find("Navigation/Sections/Viewport/Content");
    closeButton = Get<Button>("Navigation/Control/Close");
    title = GetInContent<TextMeshProUGUI>("Title");
    text = GetInContent<TextMeshProUGUI>("Text");

    if (!ComponentsInitialized()) {
      Debug.LogError("Almanac components initialization error");
      return;
    }

    closeButton.onClick.AddListener(Close);
  }

  private bool ComponentsInitialized() {
    return new object[] {
      menu, navigation, closeButton, title, text
    }.All(x => x != null);
  }

  private void OnDestroy() {
    closeButton.onClick.RemoveListener(Close);
  }

  public void Open() {
    menu.gameObject.SetActive(true);
    Clear();
    Init();
    SceneController.OpenWindow("almanac");
  }

  public void Close() {
    menu.gameObject.SetActive(false);
    Clear();
    SceneController.CloseWindow("almanac");
  }

  private void Clear() {
    foreach (Transform child in navigation) Destroy(child.gameObject);
    title.text = "";
    text.text = "";
  }

  private void Init() {
    if (Instance.sectionPrefab == null || Instance.articlePrefab == null) return;
    List<KnowledgeInstance> allArticles = KnowledgeManager.articles;

    sections = allArticles
      .GroupBy(a => a.data.section)
      .ToDictionary(g => g.Key, g => g.ToList());

    foreach (var kvp in sections) {
      GameObject sectionObj = Instantiate(Instance.sectionPrefab, navigation);
      bool opened = kvp.Key == allArticles[0].data.section;
      Sprite icon = IconDatabase.GetIcon(sectionIcons[kvp.Key]);
      sectionObj.GetComponent<AlmanacSection>().Init(kvp.Key, kvp.Value, icon, opened);
    }

    ShowContent(allArticles[0]);
  }

  public void ShowContent(KnowledgeInstance article) {
    title.text = article.data.title;
    text.text = article.data.content;
    LayoutRebuilder.ForceRebuildLayoutImmediate(text.GetComponent<RectTransform>());

    if (article.isNew) {
      article.isNew = false;
      MapUI.Instance.UpdateAlmanacIcon();
    }
  }
}
