using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerMenuUIActions {
  public static void UpdateUnitEquipment(Unit unit, Transform panel) {
    UnitEquipment equip = unit.Equip;

    Image primarySlot = panel.Find("Primary/Image").GetComponent<Image>();
    Image armorSlot = panel.Find("Armor/Image").GetComponent<Image>();
    Image secondarySlot = panel.Find("Secondary/Image").GetComponent<Image>();
    Image additionalSlot = panel.Find("Additional/Image").GetComponent<Image>();
    primarySlot.sprite = equip.primary.icon;
    armorSlot.sprite = equip.armor.icon;

    if (equip.secondary != null) {
      secondarySlot.enabled = true;
      secondarySlot.sprite = equip.secondary.icon;
    }
    else {
      secondarySlot.enabled = false;
    }

    if (equip.additional != null) {
      additionalSlot.enabled = true;
      additionalSlot.sprite = equip.additional.icon;
    }
    else {
      additionalSlot.enabled = false;
    }
  }

  public static void SwitchUnitInSquad(GameObject mark) {
    MenuSlot slot = PlayerMenuUI.selectedSlot;
    PlayerArmy playerArmy = Player.Instance.Army;

    Unit unit = PlayerMenuUI.selectedUnit;
    if (unit != null) {
      unit.InSquad = !unit.InSquad;
      mark.SetActive(unit.InSquad);
      PlayerMenuUIInfo.InSquadButtonLabel(unit);
    }

    SupportInstance support = PlayerMenuUI.selectedSupport;
    if (support != null) {
      int unitsInSquad = playerArmy.Supports.Where(s => s.inSquad).ToArray().Length;
      if (!support.inSquad && unitsInSquad >= playerArmy.SupportSlots) {
        _ = Toast.Show("warning", "No free slots");
        return;
      }

      support.inSquad = !support.inSquad;
      mark.SetActive(support.inSquad);
      PlayerMenuUIInfo.InSquadButtonLabel(support);
    }

    if (slot != null) slot.SwitchActiveMark();
    playerArmy.UpdateState();
  }

  public static void DismissConfirmation() {
    string title = "";
    string text = "";

    if (PlayerMenuUI.selectedUnit != null) {
      title = "Unit dismissing";
      text = "Are you sure you want to dismiss this unit?\nIt will become a regular villager and lose all accumulated levels.\nIts equipment will be moved to the player's inventory.";
    }
    else if (PlayerMenuUI.selectedSupport != null) {
      title = "Support dismissing";
      text = "Are you sure you want to dismiss this support?\nIt will become a regular villager.";
    }

    if (title == "") return;
    Dialog.Instance.Confirmation(DismissUnit, title, text);
  }

  private static void DismissUnit(bool accepted) {
    if (!accepted) return;
    Unit unit = PlayerMenuUI.selectedUnit;
    SupportInstance support = PlayerMenuUI.selectedSupport;

    if (unit != null) {
      Player.Instance.Army.DeleteUnit(unit);
      _ = Toast.Show("info", "Unit dismissed");
    }
    else if (support != null) {
      Player.Instance.Army.DeleteSupport(support.data.id, support.level);
      _ = Toast.Show("info", "Support dismissed");
    }

    PlayerMenuUI.SelectUnitsTab();
  }

  public static void OpenSelector(UnitEquipSlot slot) {
    Unit unit = PlayerMenuUI.selectedUnit;
    if (unit == null) return;

    List<Equipment> inventory = Player.Instance.Inventory.Equip;
    List<Equipment> canEquip = new() { };
    List<Equipment> notEnoughStats = new() { };

    foreach (Equipment item in inventory) {
      int allowed = unit.Equip.CanEquip(item, slot);
      if (allowed < 0) continue;
      else if (allowed == 0) notEnoughStats.Add(item);
      else canEquip.Add(item);
    }

    string title = "";
    switch (slot) {
      case UnitEquipSlot.Primary: title = "Change weapon"; break;
      case UnitEquipSlot.Armor: title = "Change armor"; break;
      case UnitEquipSlot.Secondary: title = "Change left-hand item"; break;
      case UnitEquipSlot.Additional: title = "Change additional item"; break;
    }

    Selector.List(ChangeEquipment, canEquip, notEnoughStats, title);
  }

  private static void ChangeEquipment(object item) {
    if (item is Equipment equipment) PlayerMenuUI.selectedUnit.Equip.EquipItem(equipment);
    if (item is not AdditionalItem) PlayerMenuUIInfo.UpdateUnitEquipment(PlayerMenuUI.selectedUnit);
    PlayerMenuUIInfo.ShowInfo(PlayerMenuUI.selectedUnit);
  }

  public static void IncreaseStat(CoreStat stat) {
    Unit hero = Player.Instance.Army.Units.FirstOrDefault(u => u.IsHero);
    if (hero == null) return;
    int[] increase = { 0, 0, 0 };

    switch (stat) {
      case CoreStat.Strength: increase[0] = 1; break;
      case CoreStat.Dexterity: increase[1] = 1; break;
      case CoreStat.Intelligence: increase[2] = 1; break;
    }

    Player.Instance.SetStatPoints(-1);
    hero.IncreaseStats(increase);
    PlayerMenuUIInfo.RecalculatePoints();
  }

  public static void UseItem() {
    Item item = PlayerMenuUI.selectedItem;
    if (item == null) return;
    item.Use();

    if (item.disposable) {
      Player.Instance.Inventory.RemoveItem(item);
      PlayerMenuUI.SelectInventoryTab();
    }
  }
}
