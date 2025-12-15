using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  private PlayerArmy army;

  public Transform oneHandWeaponBracing;
  public Transform twoHandWeaponBracing;
  public Transform smallShieldBracing;
  public Transform towerShieldBracing;

  private SkinnedMeshRenderer body;
  public Transform beard;
  public Transform hair;

  public List<Equipment> Equip { get; private set; } = new() { };
  public List<Item> Items { get; private set; } = new() { };

  private void Awake() {
    army = transform.GetComponent<PlayerArmy>();
    body = transform.Find("Model/Body").GetComponent<SkinnedMeshRenderer>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Unit inventory components initialization error");
    }
  }

  private void Start() {
    if (!ComponentsInitialized()) return;
    UpdateEquipment();
  }

  private bool ComponentsInitialized() {
    return new object[] {
      army, body, oneHandWeaponBracing, twoHandWeaponBracing, smallShieldBracing, towerShieldBracing
    }.All(x => x != null);
  }

  public void UpdateEquipment() {
    Unit hero = army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    UnitEquipment heroEquip = hero.Equip;

    if (heroEquip.armor != null) {
      GameObject prefab = hero.Size == ArmorSize.L
        ? heroEquip.armor.prefabL
        : heroEquip.armor.prefabM;

      if (prefab != null) {
        GameObject armorObj = Instantiate(heroEquip.armor.prefabM);

        if (armorObj.transform.Find("Model").TryGetComponent<SkinnedMeshRenderer>(out var armorMesh)) {
          Dictionary<string, Transform> boneMap = new ();
          foreach (Transform bone in body.bones) boneMap[bone.name] = bone;
          Transform[] armorBones = new Transform[armorMesh.bones.Length];

          for (int i = 0; i < armorMesh.bones.Length; i++) {
            string boneName = armorMesh.bones[i].name;
            armorBones[i] = boneMap[boneName];
          }

          armorMesh.bones = armorBones;
          armorMesh.rootBone = body.rootBone;

          if (hair != null) hair.gameObject.SetActive(!heroEquip.armor.bodyView.hideHair);
          if (beard != null) beard.gameObject.SetActive(!heroEquip.armor.bodyView.hideBeard);
          UpdateMaterials(heroEquip.armor);
        }
      }
    }

    foreach (Transform item in oneHandWeaponBracing) { Destroy(item.gameObject); }
    foreach (Transform item in twoHandWeaponBracing) { Destroy(item.gameObject); }
    foreach (Transform item in smallShieldBracing) { Destroy(item.gameObject); }
    foreach (Transform item in towerShieldBracing) { Destroy(item.gameObject); }

    Transform bracing;

    Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + heroEquip.primary.name);
    if (loadedWeapon == null) return;
    bracing = loadedWeapon.type == EquipmentType.TwoHandWeapon
      ? twoHandWeaponBracing
      : oneHandWeaponBracing;
    GameObject weaponObj = Instantiate(loadedWeapon.prefab, bracing);
    weaponObj.transform.SetParent(bracing, false);

    if (heroEquip.secondary is Armor secArmor) {
      Armor loadedShield = Resources.Load<Armor>("Armor/" + heroEquip.secondary.name);
      if (loadedShield == null) return;
      bracing = loadedShield.type == EquipmentType.TowerShield
        ? towerShieldBracing
        : smallShieldBracing;
      GameObject shieldObj = Instantiate(loadedShield.prefabM, bracing);
      shieldObj.transform.SetParent(bracing, false);
    }
    // FIXME: Обработать все типы доп. предмета
  }

  private void UpdateMaterials(Armor armor) {
    Material[] mats = body.sharedMaterials;

    if (armor.bodyView.topMaterial != null) mats[0] = armor.bodyView.topMaterial;
    if (armor.bodyView.underwearMaterial != null) mats[1] = armor.bodyView.underwearMaterial;
    if (armor.bodyView.bottomMaterial != null) mats[2] = armor.bodyView.bottomMaterial;

    body.sharedMaterials = mats;
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
    Equipment itemToRemove = Equip.FirstOrDefault(e => e.id == item.id);
    if (itemToRemove != null) {
      Equip.Remove(itemToRemove);
      UpdateState();
    }
  }

  public void RemoveItem(Item item) {
    if (item == null) return;
    Item itemToRemove = Items.FirstOrDefault(i => i.id == item.id);
    if (itemToRemove != null) {
      Items.Remove(itemToRemove);
      UpdateState();
    }
  }

  public void RemoveItems(Equipment[] items) {
    foreach (Equipment item in items) RemoveItem(item);
  }

  public void RemoveItems(Item[] items) {
    foreach (Item item in items) RemoveItem(item);
  }

  public bool HasItem(Equipment item, bool onlyUnequipped = false) {
    if (Equip.Any(i => i.id == item.id)) return true;
    if (!onlyUnequipped) {
      foreach (Unit unit in army.Units) {
        if (unit.Equip.HasItem(item)) return true;
      }
    }
    return false;
  }

  public bool HasItem(Item item) {
    return Items.Any(i => i.id == item.id);
  }

  public bool HasItems(Equipment[] items, bool onlyUnequipped = false) {
    var grouped = items
      .GroupBy(i => i.id)
      .Select(g => new { Item = g.First(), Count = g.Count() });

    foreach (var req in grouped) {
      if (GetEquipmentCount(req.Item, onlyUnequipped) < req.Count) return false;
    }

    return true;
  }

  public bool HasItems(Item[] items) {
    var grouped = items
      .GroupBy(i => i.id)
      .Select(g => new { Item = g.First(), Count = g.Count() });

    foreach (var req in grouped) {
      if (GetItemCount(req.Item) < req.Count) return false;
    }

    return true;
  }

  private int GetEquipmentCount(Equipment item, bool onlyUnequipped = false) {
    int count = Equip.Count(i => i.id == item.id);
    if (!onlyUnequipped) {
      foreach (Unit unit in army.Units) {
        if (unit.Equip.HasItem(item)) count++;
      }
    }
    return count;
  }

  private int GetItemCount(Item item) {
    return Items.Count(i => i.id == item.id);
  }

  public void UpdateState() {
    StateManager.inventoryEquipment = Equip.ToArray();
    StateManager.inventoryItems = Items.ToArray();
  }
}
