using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerArmy : MonoBehaviour {
  public List<Unit> Units { get; private set; } = new();
  public List<SupportInstance> Supports { get; private set; } = new();
  public int SupportSlots { get; private set; } = 1;

  void OnDestroy() {
    Units.Clear();
    Supports.Clear();
  }

  public void UpdateUnits(UnitData[] array) {
    Units = array.Select(data => {
      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return null;
      unit.FromData(data);
      if (unit.CurrentHealth == 0 && unit.IsHero) unit.CurrentHealth = 1f;
      return unit;
    }).ToList();
  }

  public void UpdateSupports(SupportData[] array) {
    Supports = array.Select(data => {
      Support unit = Factory.CreateSupportById(data.id);
      if (unit == null) return null;
      SupportInstance support = new(unit, data.level);
      support.FromData(data);
      return support;
    }).ToList();
  }

  public void AddUnit(Unit unit) {
    if (unit == null) return;
    Unit prefab = StateManager.PrefabDatabase.GetPrefab(unit.PrefabId, true);
    prefab.InSquad = false;
    prefab.IsNew = true;
    prefab.CurrentHealth = prefab.TotalHealth;
    Units.Add(prefab);
    UpdateState();
  }

  public void DeleteUnit(Unit unit) {
    for (int i = 0; i < Units.Count; i++) {
      if (Units[i] == unit) {
        unit.Equip.UnequipAll();
        Destroy(Units[i]);
        Units.RemoveAt(i);
        break;
      }
    }

    UpdateState();
  }

  public void AddSupport(Support data, MasteryLevel level) {
    SupportInstance instance = new(data, level) { isNew = true };
    Supports.Add(instance);
    UpdateState();
  }

  public void DeleteSupport(string id, MasteryLevel level) {
    for (int i = 0; i < Supports.Count; i++) {
      if (Supports[i].data.id == id && Supports[i].level == level) {
        Supports.RemoveAt(i);
        break;
      }
    }
    UpdateState();
  }

  public void UpdateState() {
    StateManager.WriteUnitsData(Units.ToArray(), "allies");
    StateManager.WriteSupportsData(Supports.ToArray());
  }

  public bool HasUnit(Unit unit) {
    return Units.Any(u => u.PrefabId == unit.PrefabId);
  }

  public bool HasSupport(string id) {
    return Supports.Exists(s => s.data.id == id);
  }

  public bool SupportInSquad(string id) {
    return Supports.Exists(s => s.data.id == id && s.inSquad);
  }

  public void SetSupportSlots(int value) {
    SupportSlots += value;
    if (SupportSlots > 3) SupportSlots = 3;
    StateManager.supportSlots = SupportSlots;
  }
}
