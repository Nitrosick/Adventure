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

  private static string GetPath(string id) {
    if (string.IsNullOrEmpty(id)) return null;
    if (id.StartsWith("a") || id.StartsWith("s")) return "Armor/" + id;
    if (id.StartsWith("w")) return "Weapon/" + id;
    if (id.StartsWith("mi")) return "Medicine/" + id;
    // FIXME: Добавить все каталоги предметов
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
