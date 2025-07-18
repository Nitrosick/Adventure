using System.Collections.Generic;
using UnityEngine;

public static class Factory {
  public static Equipment CreateById(string id) {
    if (string.IsNullOrEmpty(id)) return null;

    string path = "";

    if (id.StartsWith("a") || id.StartsWith("s")) path = "Armor/" + id;
    else if (id.StartsWith("w")) path = "Weapon/" + id;
    else {
      Debug.LogError("Item not found");
      return null;
    }

    Equipment item = Resources.Load<Equipment>(path);
    if (item == null) Debug.LogError("Item not found");
    return item;
  }

  public static Equipment[] CreateById(string[] ids) {
    List<Equipment> result = new ();
    if (ids == null || ids.Length == 0) return result.ToArray();

    foreach (string id in ids) {
      string path = "";

      if (id.StartsWith("a") || id.StartsWith("s")) path = "Armor/" + id;
      else if (id.StartsWith("w")) path = "Weapon/" + id;
      else {
        Debug.LogError("Item not found");
        continue;
      }

      Equipment item = Resources.Load<Equipment>(path);
      if (item == null) {
        Debug.LogError("Item not found");
        continue;
      }
      result.Add(item);
    }

    return result.ToArray();
  }
}
