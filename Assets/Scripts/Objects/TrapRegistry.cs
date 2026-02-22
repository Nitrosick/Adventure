using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/TrapRegistry")]
public class TrapRegistry : ScriptableObject {
  public List<TrapEntry> traps;

  private Dictionary<TrapType, GameObject> lookup;

  public GameObject Get(TrapType type) {
    lookup ??= traps.ToDictionary(t => t.type, t => t.prefab);
    return lookup[type];
  }
}

[System.Serializable]
public class TrapEntry {
  public TrapType type;
  public GameObject prefab;
}
