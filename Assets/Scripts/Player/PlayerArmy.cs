using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerArmy : MonoBehaviour {
  public Unit[] Units { get; private set; } = { };

  private void Awake() {
    // Default player army
    BattleResult? result = StateManager.battleResult;
    if (Units.Length > 0 || result != null) return;

    PrefabDatabase database = Resources.Load<PrefabDatabase>("Databases/PrefabDatabase");
    int[] unitIds = { 1, 2, 2 };

    List<Unit> temp = new();
    foreach (int id in unitIds) {
      Unit prefab = database.GetPrefab(id, true);
      temp.Add(prefab);
    }
    Units = temp.ToArray();
  }

  public void UpdateUnits(UnitData[] array) {
    Units = array.Select(data => {
      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return null;
      unit.FromData(data);
      return unit;
    }).ToArray();
  }

  public void UpdateUnitsHPAfterDefeat() {
    Unit[] active = Units.Where(u => u.InSquad).ToArray();
    foreach (Unit unit in active) {
      unit.CurrentHealth = 1;
    }
  }

  public void SwitchUnitInSquad(Unit unit) {
    Unit selected = Array.Find(Units, u => u == unit);
    if (selected == null) return;
    selected.InSquad = !selected.InSquad;
  }
}
