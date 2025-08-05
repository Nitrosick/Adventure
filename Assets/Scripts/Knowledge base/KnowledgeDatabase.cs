using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "KnowledgeDatabase", menuName = "KnowledgeBase/Database")]
public class KnowledgeDatabase : ScriptableObject {
  public List<KnowledgeArticle> articles;
}
