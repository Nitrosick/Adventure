using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/IconDatabase")]
public class IconDatabase : ScriptableObject
{
  [System.Serializable]
  public class Entry {
    public string name;
    public Sprite icon;
  }

  public Entry[] entries;

  private Dictionary<string, Sprite> _map;

  private void OnEnable() {
    _map = new Dictionary<string, Sprite>();
    foreach (var entry in entries) _map[entry.name] = entry.icon;
  }

  public Sprite GetIcon(string name) {
    if (_map.TryGetValue(name, out var icon)) return icon;
    Debug.LogError("Icon not found");
    return null;
  }
}
