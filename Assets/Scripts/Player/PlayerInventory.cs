using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  private PlayerArmy army;
  public Transform weaponBracing;
  public Transform shieldBracing;

  public List<Equipment> Equip { get; private set; } = new() { };
  public List<Item> Items { get; private set; } = new() { };
  private ArmorSet[] armorSets;

  private void Awake() {
    army = transform.GetComponent<PlayerArmy>();
    armorSets = GetComponentsInChildren<ArmorSet>();

    if (army == null || armorSets.Length == 0) {
      Debug.LogError("Unit inventory components initialization error");
    }
  }

  private void Start() {
    UpdateEquipment();
  }

  public void UpdateEquipment() {
    Unit hero = army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    UnitEquipment heroEquip = hero.Equip;

    foreach (ArmorSet set in armorSets) {
      if (set.id == heroEquip.armor.id) set.gameObject.SetActive(true);
      else set.gameObject.SetActive(false);
    }

    foreach (Transform item in weaponBracing) { Destroy(item.gameObject); }
    foreach (Transform item in shieldBracing) { Destroy(item.gameObject); }

    Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + heroEquip.primaryWeapon.name);
    Armor loadedShield = Resources.Load<Armor>("Armor/" + heroEquip.shield.name);
    if (loadedWeapon == null || loadedShield == null) return;

    GameObject weaponObj = Instantiate(loadedWeapon.prefab, weaponBracing);
    weaponObj.transform.SetParent(weaponBracing, false);
    GameObject shieldObj = Instantiate(loadedShield.prefab, shieldBracing);
    shieldObj.transform.SetParent(shieldBracing, false);
  }

  public void UpdateInventory(Equipment[] items) { Equip = items.ToList(); }
  public void UpdateInventory(Item[] items) { Items = items.ToList(); }

  public void AddItems(List<Equipment> items) {
    if (items == null || items.Count == 0) return;
    Equip.AddRange(items);
    UpdateState();
  }

  public void AddItems(Equipment item) {
    if (item == null) return;
    Equip.Add(item);
    UpdateState();
  }

  public void AddItems(List<Item> items) {
    if (items == null || items.Count == 0) return;
    Items.AddRange(items);
    UpdateState();
  }

  public void AddItems(Item item) {
    if (item == null) return;
    Items.Add(item);
    UpdateState();
  }

  public void RemoveItem(Equipment item) {
    if (item == null) return;
    Equip.Remove(item);
    UpdateState();
  }

  public void RemoveItem(Item item) {
    if (item == null) return;
    Items.Remove(item);
    UpdateState();
  }

  public void UpdateState() {
    StateManager.inventoryEquipment = Equip.ToArray();
    StateManager.inventoryItems = Items.ToArray();
  }
}
