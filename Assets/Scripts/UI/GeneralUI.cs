using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUI : MonoBehaviour {
  protected Button mainMenuButton;
  protected Button almanacButton;
  protected Image almanacIcon;

  protected Color defaultColor;
  protected Color highlightedColor;

  protected T Get<T>(Transform parent, string path) where T : Component {
    return parent.Find(path).GetComponent<T>();
  }

  protected GameObject Find(Transform parent, string path) {
    return parent.Find(path).gameObject;
  }

  protected virtual void Awake() {
    Transform top = transform.Find("Top");
    Transform mainMenu = top.Find("MainMenu");

    mainMenuButton = Get<Button>(mainMenu, "Main");
    almanacButton = Get<Button>(mainMenu, "Almanac");
    almanacIcon = Get<Image>(mainMenu, "Almanac/Icon");

    if (mainMenuButton == null || almanacButton == null || almanacIcon == null) {
      Debug.LogError("UI components initialization error");
    }

    mainMenuButton.onClick.AddListener(OpenPauseMenu);
    almanacButton.onClick.AddListener(OpenAlmanac);

    ColorUtility.TryParseHtmlString("#4B4A47", out defaultColor);
    ColorUtility.TryParseHtmlString("#EFBF0D", out highlightedColor);
  }

  private void Start() {
    UpdateAlmanacIcon();
  }

  protected virtual void OnDestroy() {
    mainMenuButton.onClick.RemoveListener(OpenPauseMenu);
    almanacButton.onClick.RemoveListener(OpenAlmanac);
  }

  public virtual void DisableUI() {
    mainMenuButton.interactable = false;
    almanacButton.interactable = false;
  }

  public virtual void EnableUI() {
    mainMenuButton.interactable = true;
    almanacButton.interactable = true;
  }

  protected virtual void OpenPauseMenu() {}
  protected virtual void OpenAlmanac() {}

  public void UpdateAlmanacIcon() {
    if (almanacIcon == null) return;
    if (KnowledgeManager.articles.Any(a => a.isNew)) almanacIcon.color = highlightedColor;
    else almanacIcon.color = defaultColor;
  }
}
