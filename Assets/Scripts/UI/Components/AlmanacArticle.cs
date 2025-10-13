using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlmanacArticle : MonoBehaviour {
  private Button button;
  private TextMeshProUGUI title;
  private GameObject newIcon;
  private KnowledgeInstance currentArticle;

  private void Awake() {
    button = transform.GetComponent<Button>();
    title = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    newIcon = transform.Find("New").gameObject;

    if (button == null || title == null || newIcon == null) {
      Debug.LogError("Almanac article components initialization error");
      return;
    }

    button.onClick.AddListener(ShowArticleContent);
  }

  private void OnDestroy() {
    button.onClick.RemoveListener(ShowArticleContent);
  }

  public void Init(KnowledgeInstance article) {
    currentArticle = article;
    title.text = article.data.title;
    if (article.isNew) newIcon.SetActive(true);
  }

  private void ShowArticleContent() {
    AlmanacUI.Instance.ShowContent(currentArticle);
    newIcon.SetActive(false);
  }
}
