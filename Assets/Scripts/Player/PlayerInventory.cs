using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  public List<Equipment> Equip { get; private set; } = new() { };
  // FIXME: Отдельный список для предметов

  public void AddItems(List<Equipment> items) {
    if (items == null || items.Count == 0) return;
    Equip.AddRange(items);
  }

  public void AddItems(Equipment item) {
    if (item == null) return;
    Equip.Add(item);
  }
}
