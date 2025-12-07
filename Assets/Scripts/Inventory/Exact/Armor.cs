using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Equipment/Armor")]
public class Armor : Equipment {
  public float defense;
  public float blockMultiplier = 1f;
  public GameObject prefab;
  public List<DamageResistance> resistsMap;
  public Dictionary<DamageType, float> resists;
  // FIXME: Добавить все типы резистов
  public bool hideBeard;
  public bool hideHair;

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
