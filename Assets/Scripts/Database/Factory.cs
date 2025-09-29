using System;
using System.Linq;
using UnityEngine;

public static class Factory {
  public static Equipment CreateEquipById(string id) {
    return Load<Equipment>(GetPath(id));
  }

  public static Equipment[] CreateEquipById(string[] ids) {
    if (ids == null || ids.Length == 0) return Array.Empty<Equipment>();

    return ids
      .Select(id => Load<Equipment>(GetPath(id)))
      .Where(e => e != null)
      .ToArray();
  }

  public static Item CreateItemById(string id) {
    return Load<Item>(GetPath(id));
  }

  public static Item[] CreateItemById(string[] ids) {
    if (ids == null || ids.Length == 0) return Array.Empty<Item>();

    return ids
      .Select(id => Load<Item>(GetPath(id)))
      .Where(i => i != null)
      .ToArray();
  }

  public static Support CreateSupportById(string id) {
    return Load<Support>(GetPath(id));
  }

  public static Quest CreateQuestById(string id) {
    return Load<Quest>(GetPath(id));
  }

  public static KnowledgeArticle CreateArticleById(string id) {
    return Load<KnowledgeArticle>(GetPath(id));
  }

  public static Ability CreateAbilityById(string id) {
    return Load<Ability>(GetPath(id));
  }

  private static string GetPath(string id) {
    if (string.IsNullOrEmpty(id)) return null;
    if (id.StartsWith("aa")) return "Knowledge/" + id;
    if (id.StartsWith("ab")) return "Abilities/" + id;
    if (id.StartsWith("ai")) return "Additional/" + id;
    if (id.StartsWith("su")) return "Supports/" + id;
    if (id.StartsWith("mi") || id.StartsWith("g")) return "Misc/" + id;
    if (id.StartsWith("a") || id.StartsWith("s")) return "Armor/" + id;
    if (id.StartsWith("q")) return "Quests/" + id;
    if (id.StartsWith("w")) return "Weapon/" + id;
    Debug.LogError($"Unknown equipment id: {id}");
    return null;
  }

  private static T Load<T>(string path) where T : UnityEngine.Object {
    if (string.IsNullOrEmpty(path)) return null;
    T asset = Resources.Load<T>(path);
    if (asset == null) Debug.LogError("Failed to load resource");
    return asset;
  }
}
