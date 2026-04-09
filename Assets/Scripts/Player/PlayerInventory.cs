using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  private PlayerArmy army;
  private PlayerAnimator animator;

  private GameObject armorObj;
  public Transform spineBracing;
  public Transform hipsBracing;
  public GameObject torch;

  private SkinnedMeshRenderer body;
  private CapsuleCollider[] clothColliders = {};
  public Transform beard;
  public Transform hair;

  public List<Equipment> Equip { get; private set; } = new() { };
  public List<Item> Items { get; private set; } = new() { };

  void Awake() {
    army = transform.GetComponent<PlayerArmy>();
    animator = transform.GetComponent<PlayerAnimator>();
    body = transform.Find("Model/Body").GetComponent<SkinnedMeshRenderer>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Unit inventory components initialization error");
    }
  }

  void Start() {
    if (!ComponentsInitialized()) return;
    UpdateEquipment();
  }

  private bool ComponentsInitialized() {
    return new object[] {
      army, animator, body, spineBracing, hipsBracing, torch
    }.All(x => x != null);
  }

  public void UpdateEquipment() {
    Unit hero = army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    UnitEquipment heroEquip = hero.Equip;

    if (armorObj != null) {
      Destroy(armorObj);
      armorObj = null;
    }

    if (heroEquip.armor != null) {
      GameObject prefab = hero.Size == ArmorSize.L
        ? heroEquip.armor.prefabL
        : heroEquip.armor.prefabM;

      if (prefab != null) {
        armorObj = Instantiate(heroEquip.armor.prefabM);
        Transform model = armorObj.transform.Find("Model");
        Transform cape = armorObj.transform.Find("Cape");

        CapsuleCollider hipsCollider = transform.Find("Model/Armature/mixamorig:Hips").GetComponent<CapsuleCollider>();
        CapsuleCollider spineCollider = transform.Find("Model/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2").GetComponent<CapsuleCollider>();

        if (hipsCollider != null && spineCollider != null) {
          clothColliders = new CapsuleCollider[] {
            hipsCollider, spineCollider
          };
        }

        if (cape != null) {
          if (cape.TryGetComponent<SkinnedMeshRenderer>(out var capeMesh)) RetargetBones(capeMesh);
          if (cape.TryGetComponent<Cloth>(out var cloth)) cloth.capsuleColliders = clothColliders;
        }

        if (model != null && model.TryGetComponent<SkinnedMeshRenderer>(out var armorMesh)) {
          RetargetBones(armorMesh);
          if (hair != null) hair.gameObject.SetActive(!heroEquip.armor.bodyView.hideHair);
          if (beard != null) beard.gameObject.SetActive(!heroEquip.armor.bodyView.hideBeard);
        }
      }
    }

    foreach (Transform item in spineBracing) { Destroy(item.gameObject); }
    foreach (Transform item in hipsBracing) { Destroy(item.gameObject); }

    if (heroEquip.primary != null) {
      Weapon loadedWeapon = Resources.Load<Weapon>("Weapon/" + heroEquip.primary.name);
      if (loadedWeapon == null) return;
      Transform bracing = GetBracing(heroEquip.primary.bracingType);
      GameObject weaponObj = Instantiate(loadedWeapon.prefab, bracing);
      weaponObj.transform.SetParent(bracing, false);
      weaponObj.transform.localPosition = heroEquip.primary.bracingLocation;
      weaponObj.transform.localEulerAngles = heroEquip.primary.bracingRotation;
    }

    if (heroEquip.secondary is Armor secArmor) {
      Armor loadedShield = Resources.Load<Armor>("Armor/" + heroEquip.secondary.name);
      if (loadedShield == null) return;
      Transform bracing = GetBracing(heroEquip.secondary.bracingType);
      GameObject shieldObj = Instantiate(loadedShield.prefabM, bracing);
      shieldObj.transform.SetParent(bracing, false);
      shieldObj.transform.localPosition = heroEquip.secondary.bracingLocation;
      shieldObj.transform.localEulerAngles = heroEquip.secondary.bracingRotation;
    }
    // TODO: Обработать все типы доп. предмета

    UpdateMaterials(heroEquip.armor);
  }

  private void RetargetBones(SkinnedMeshRenderer mesh) {
    Dictionary<string, Transform> boneMap = new ();
    foreach (Transform bone in body.bones) boneMap[bone.name] = bone;
    Transform[] armorBones = new Transform[mesh.bones.Length];

    for (int i = 0; i < mesh.bones.Length; i++) {
      string boneName = mesh.bones[i].name;
      armorBones[i] = boneMap[boneName];
    }

    mesh.bones = armorBones;
    mesh.rootBone = body.rootBone;
  }

  private void UpdateMaterials(Armor armor) {
    Material[] mats = body.sharedMaterials;
    Material[] materials;
    GameManager mng = GameManager.I;

    if (armor != null) {
      BodyView bv = armor.bodyView;

      materials = new Material[] {
        bv.torsoMaterial,
        bv.underwearMaterial,
        bv.legsMaterial,
        bv.footsMaterial,
        bv.armsMaterial,
        bv.handsMaterial
      };
    } else {
      materials = new Material[] {
        mng.leatherMaterial,
        mng.leatherMaterial,
        mng.leatherMaterial,
        mng.skinMaterial,
        mng.skinMaterial,
        mng.skinMaterial
      };
    }

    for (int i = 0; i < materials.Length; i++) {
      if (materials[i] == null) continue;
      mats[i] = materials[i];
    }

    body.sharedMaterials = mats;
  }

  private Transform GetBracing(BracingType type) {
    return type switch {
      BracingType.Spine => spineBracing,
      BracingType.Hips => hipsBracing,
      _ => null,
    };
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

  public void EquipTorch() {
    animator.SetTorch(true);
    torch.SetActive(true);
  }

  public void UnequipTorch() {
    animator.SetTorch(false);
    torch.SetActive(false);
  }
}
