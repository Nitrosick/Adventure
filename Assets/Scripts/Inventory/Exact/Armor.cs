using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyView {
  public bool hideBeard;
  public bool hideHair;
  public Material handsMaterial;
  public Material armsMaterial;
  public Material torsoMaterial;
  public Material underwearMaterial;
  public Material legsMaterial;
  public Material footsMaterial;
}

[CreateAssetMenu(menuName = "GameObjects/Equipment/Armor")]
public class Armor : Equipment {
  public float defense;
  public float blockMultiplier = 1f;
  // public GameObject prefabS;
  public GameObject prefabM;
  public GameObject prefabL;
  public List<DamageResistance> resistsMap;
  public Dictionary<DamageType, float> resists;
  // FIXME: Добавить все типы резистов
  public BodyView bodyView;

  private void OnEnable() {
    resists = new Dictionary<DamageType, float>();
    foreach (var res in resistsMap) resists[res.type] = res.value;
  }

  private void OnValidate() {
    if (resistsMap != null && resistsMap.Count > 0) return;

    resistsMap = new List<DamageResistance>();
    foreach (DamageType damage in Enum.GetValues(typeof(DamageType))) {
      if (damage == DamageType.No) continue;
      resistsMap.Add(new DamageResistance { type = damage, value = 0f });
    }
  }
}
