using UnityEngine;

[CreateAssetMenu(fileName = "Article", menuName = "KnowledgeBase/Article")]
public class KnowledgeArticle : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string content;
  public Sprite icon;
  public bool unlocked;
  public bool isNew = true;
}
