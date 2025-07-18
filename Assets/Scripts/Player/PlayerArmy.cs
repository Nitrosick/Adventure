using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerArmy : MonoBehaviour {
  public List<Unit> Units { get; private set; } = new();

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
