using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerArmy : MonoBehaviour {
  public List<Unit> Units { get; private set; } = new();

  private void Awake() {
    // Default player army
    BattleResult? result = StateManager.battleResult;
    if (Units.Count > 0 || result != null) return;

    PrefabDatabase database = Resources.Load<PrefabDatabase>("Databases/PrefabDatabase");
    int[] unitIds = { 1, 2, 2 };

    foreach (int id in unitIds) {
      Unit prefab = database.GetPrefab(id, true);
      Units.Add(prefab);
    }
  }

  public void UpdateUnits(UnitData[] array) {
    Units = array.Select(data => {
      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return null;
      unit.FromData(data);
      return unit;
    }).ToList();
  }

  public void UpdateUnitsHPAfterDefeat() {
    Unit[] active = Units.Where(u => u.InSquad).ToArray();
    foreach (Unit unit in active) {
      unit.CurrentHealth = 1;
    }
  }

  public void DeleteUnit(Unit unit) {
    for (int i = 0; i < Units.Count; i++) {
      if (Units[i] == unit) {
        unit.Equip.UnequipAll();
        Destroy(Units[i]);
        Units.RemoveAt(i);
        return;
      }
    }
  }
}
