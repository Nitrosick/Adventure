[System.Serializable]
public class KnowledgeInstance {
  public KnowledgeArticle data;
  public bool isNew;

  public KnowledgeInstance(KnowledgeArticle _data, bool _isNew = true) {
    data = _data;
    isNew = _isNew;
  }
}
