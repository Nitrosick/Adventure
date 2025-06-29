using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  public List<Equipment> Equip { get; private set; } = new() {};
  // FIXME: Отдельный список для предметов

  private void Awake() {

  }
}
