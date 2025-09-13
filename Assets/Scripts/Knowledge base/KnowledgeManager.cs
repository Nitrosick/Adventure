using System.Collections.Generic;
using UnityEngine;

public class KnowledgeManager : MonoBehaviour {
  public static List<KnowledgeInstance> articles = new ();

  private void Start() {
    GetStateData();
  }

  private void OnDestroy() {
    articles.Clear();
  }

  public static void UnlockArticle(string id) {
    KnowledgeArticle article = Factory.CreateArticleById(id);
    if (article == null) return;
    KnowledgeInstance articleIns = new(article);
    articles.Add(articleIns);
    StateManager.unlockedKnowledge.Add(id);
  }

  public static void UnlockArticle(string[] ids) {
    foreach (string id in ids) {
      KnowledgeArticle article = Factory.CreateArticleById(id);
      if (article == null) continue;
      KnowledgeInstance articleIns = new(article);
      articles.Add(articleIns);
      StateManager.unlockedKnowledge.Add(id);
    }
  }

  private static void GetStateData() {
    HashSet<string> data = StateManager.unlockedKnowledge;
    articles.Clear();

    if (data.Count == 0) {
      UnlockArticle(new string[] { "aa1", "aa2" });
      articles[0].isNew = false;
      Dialog.Info(articles[0].data.title, articles[0].data.content, "Continue");
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
