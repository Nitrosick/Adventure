using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
  [System.Serializable]
  public class Entry {
    public int id;
    public Unit prefab;
  }

  public Entry[] entries;

  private Dictionary<int, Unit> _map;

  private void OnEnable() {
    _map = new Dictionary<int, Unit>();
    foreach (var entry in entries) _map[entry.id] = entry.prefab;
  }

  public Unit GetPrefab(int id, bool isVirtual = false) {
    if (_map.TryGetValue(id, out var prefab)) {
      Unit instance = Instantiate(prefab);
      if (isVirtual) instance.gameObject.SetActive(false);
      return instance;
    }
    Debug.LogError("Prefab not found");
    return null;
  }
}
