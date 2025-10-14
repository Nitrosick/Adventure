using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AlmanacSection : MonoBehaviour {
  private Button switcher;
  private TextMeshProUGUI switcherTitle;
  private Image switcherIcon;
  private GameObject switcherNew;
  private Transform articlesList;
  private KnowledgeSection section;
  private bool opened;

  private void Awake() {
    switcher = transform.Find("Switcher").GetComponent<Button>();
    switcherTitle = transform.Find("Switcher/Text").GetComponent<TextMeshProUGUI>();
    switcherIcon = transform.Find("Switcher/Icon").GetComponent<Image>();
    switcherNew = transform.Find("Switcher/New").gameObject;
    articlesList = transform.Find("List");

    if (
      switcher == null || switcherTitle == null || switcherIcon == null ||
      switcherNew == null || articlesList == null
    ) {
      Debug.LogError("Almanac section components initialization error");
      return;
    }

    switcher.onClick.AddListener(SwitchSection);
  }

  private void OnDestroy() {
    switcher.onClick.RemoveListener(SwitchSection);
  }

  public void Init(KnowledgeSection group, List<KnowledgeInstance> list, Sprite icon, bool open = false) {
    section = group;
    switcherTitle.text = Utils.SplitPascalCase(group.ToString());

    if (list.Count > 0) {
      switcherIcon.sprite = icon;
      if (list.Any(a => a.isNew)) switcherNew.SetActive(true);

      foreach (KnowledgeInstance article in list) {
        GameObject articleObj = Instantiate(AlmanacUI.Instance.articlePrefab, articlesList);
        articleObj.GetComponent<AlmanacArticle>().Init(article);
      }

      opened = open;
      articlesList.gameObject.SetActive(open);
    } else {
      switcherIcon.gameObject.SetActive(false);
      switcher.interactable = false;
    }
  }

  private void SwitchSection() {
    opened = !opened;
    articlesList.gameObject.SetActive(opened);
  }

  public void CheckNewArticles() {
    if (KnowledgeManager.articles.Any(a => a.data.section == section && a.isNew)) switcherNew.SetActive(true);
    else switcherNew.SetActive(false);
  }
}
