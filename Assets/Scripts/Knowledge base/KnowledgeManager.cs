using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnowledgeManager : MonoBehaviour {
  public static KnowledgeManager Instance;
  public KnowledgeDatabase database;

  private void Awake() {
    Instance = this;
  }

  private void Start() {
    GetStateData();
  }

  public static void UnlockArticle(string id) {
    KnowledgeArticle article = Instance.database.articles
      .FirstOrDefault(a => a.id == id);

    if (article == null) {
      Debug.LogError("Almanach article not found");
      return;
    }

    if (!article.unlocked) {
      article.unlocked = true;
      StateManager.unlockedKnowledge.Add(id);
    }
  }

  public static List<KnowledgeArticle> GetUnlockedArticles() {
    return Instance.database.articles
      .Where(a => a.unlocked)
      .ToList();
  }

  // public bool IsArticleUnlocked(string articleId) {
  //   var article = database.articles.FirstOrDefault(a => a.id == articleId);
  //   return article != null && article.unlocked;
  // }

  private static void GetStateData() {
    HashSet<string> articles = StateManager.unlockedKnowledge;

    if (articles.Count == 0) {
      UnlockArticle("aa1");
      // FIXME: Выводить сообщение welcome
    }

    foreach (KnowledgeArticle a in Instance.database.articles) {
      if (articles.Contains(a.id)) {
        a.unlocked = true;
      }
    }
  }
}
