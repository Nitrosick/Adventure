using System.Collections.Generic;
using UnityEngine;

public class KnowledgeManager : MonoBehaviour {
  public static List<KnowledgeInstance> articles = new ();

  void Start() {
    GetStateData();
  }

  void OnDestroy() {
    articles.Clear();
  }

  public static void UnlockArticle(string id) {
    if (StateManager.unlockedKnowledge.Contains(id)) return;
    KnowledgeArticle article = Factory.CreateArticleById(id);
    if (article == null) return;
    KnowledgeInstance articleIns = new(article);
    articles.Add(articleIns);
    StateManager.unlockedKnowledge.Add(id);
    MapUI.Instance.UpdateAlmanacIcon();
  }

  public static void UnlockArticle(string[] ids) {
    foreach (string id in ids) UnlockArticle(id);
  }

  private static void GetStateData() {
    HashSet<string> data = StateManager.unlockedKnowledge;
    articles.Clear();

    if (data.Count == 0) {
      UnlockArticle(StateManager.defaultKnowledge);
      articles[0].isNew = false;
      Dialog.Instance.Info(articles[0].data.title, articles[0].data.content, "Continue");
    } else {
      foreach (string id in data) {
        KnowledgeArticle article = Factory.CreateArticleById(id);
        if (article == null) continue;
        KnowledgeInstance articleIns = new(article, false);
        articles.Add(articleIns);
      }
    }

    MapUI.Instance.UpdateAlmanacIcon();
  }
}
