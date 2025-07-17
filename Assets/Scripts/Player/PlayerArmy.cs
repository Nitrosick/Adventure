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
    string[] unitIds = { "u1", "u2", "u2" };

    foreach (string id in unitIds) {
      Unit prefab = database.GetPrefab(id, true);
      Units.Add(prefab);
    }

    UpdateState();
  }

  public void UpdateUnits(UnitData[] array) {
    Units = array.Select(data => {
      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return null;
      unit.FromData(data);
      return unit;
    }).ToList();

    foreach (Unit unit in Units) {
      if (unit.CurrentHealth <= 0) {
        if (unit.IsHero) unit.CurrentHealth = 1f;
        else unit.InSquad = false;
      }
    }

    UpdateState();
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

    UpdateState();
  }

  public void UpdateState() {
    StateManager.WriteUnitsData(Units.ToArray(), "allies");
  }
}
