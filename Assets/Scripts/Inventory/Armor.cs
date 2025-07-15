using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Armor")]
public class Armor : Equipment {
  public int defense;
  public GameObject prefab;
  public List<DamageResistance> resistsMap;
  public Dictionary<DamageType, float> resists;
  // FIXME: Добавить все типы резистов

  private void OnEnable() {
    resists = new Dictionary<DamageType, float>();
    foreach (var res in resistsMap) resists[res.type] = res.value;
  }

  private void OnValidate() {
    if (resistsMap != null && resistsMap.Count > 0) return;

    resistsMap = new List<DamageResistance>();
    foreach (DamageType damage in System.Enum.GetValues(typeof(DamageType))) {
      if (damage == DamageType.No) continue;
      resistsMap.Add(new DamageResistance { type = damage, value = 0f });
    }
  }
}
