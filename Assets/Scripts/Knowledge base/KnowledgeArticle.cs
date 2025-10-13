using UnityEngine;

[CreateAssetMenu(fileName = "Article", menuName = "GameObjects/KnowledgeBase/Article")]
public class KnowledgeArticle : ScriptableObject {
  public string id;
  public string title;
  [TextArea(5, 20)] public string content;
  public KnowledgeSection section;
}
