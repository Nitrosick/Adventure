using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UnitSkills : MonoBehaviour {
  private Unit unit;

  void Awake() {
    unit = transform.GetComponent<Unit>();

    if (!ComponentsInitialized()) {
      Debug.LogError("Unit skills components initialization error");
    }
  }

  private bool ComponentsInitialized() {
    return new object[] {
      unit
    }.All(x => x != null);
  }

  public List<Skill> GetActiveSkills() => GetSkills(true);
  public List<Skill> GetPassiveSkills() => GetSkills(false);

  private List<Skill> GetSkills(bool active) {
    UnitEquipment equip = unit.Equip;

    return new[] { equip.primary, equip.secondary, equip.armor, equip.additional }
      .Where(e => e != null && e.skills != null)
      .SelectMany(e => e.skills)
      .Concat(unit.Effects.innateSkills)
      .Where(s => s != null && s.isActive == active)
      .ToList();
  }

  // public bool HasAttackPhaseSkills() {
  //   if (unit.SkillCharges == 0) return false;

  //   foreach (Skill skill in GetActiveSkills()) {
  //     // FIXME: Может сломаться проверка, если использовать не в PhaseManager
  //     if (skill.skillName == "Charged attack") continue;
  //     if (unit.Effects.HasAnyEffect(new string[] { "Stun", "Root" }) && !skill.canUseInRoot) continue;
  //     if (skill.skillPhases.Contains(BattlePhase.Attack)) return true;
  //   }
  //   return false;
  // }

  public bool HasNonTargetSkills() {
    if (unit.Effects.HasEffect("Stun")) return false;
    bool isRooted = unit.Effects.HasEffect("Root");

    return GetActiveSkills().Any(s =>
      s.isActive &&
      !s.needTarget &&
      s.skillPhases.Contains(BattlePhase.Attack) &&
      (!isRooted || s.canUseInRoot)
    );
  }

  public void ApplyInstantEffects() {
    List<Skill> skills = GetPassiveSkills();
    if (skills.Count == 0) return;
    string id = "";

    foreach (Skill skill in skills) {
      switch (skill.skillName) {
        case "Inspiration": id = "e4"; break;
        case "Night life": if (TimeController.Instance.IsNight()) id = "e10"; break;
        case "Stealth": if (TimeController.Instance.IsNight()) id = "e11"; break;
      }

      if (id == "") continue;
      Effect effect = Factory.CreateEffectById(id);
      if (effect != null) unit.Effects.ApplyEffect(effect);
    }
  }

  private void AfterSkillUse() {
    if (unit.SkillCharges <= 0) BattleUI.Instance.DisableSkills();
    if (unit.Skills.GetActiveSkills().Count > 0) unit.Ui.UpdateCharges(unit.TotalSkillCharges, unit.SkillCharges);
    // unit.NextPhase(true);
    _ = QueueManager.Instance.NextUnit();
  }

  public void BlockStance(string id) {
    Effect effect = Factory.CreateEffectById(id);
    if (effect == null) return;

    unit.Effects.ApplyEffect(effect);
    unit.Animator.SetBlocking(true);
    AfterSkillUse();
  }

  public async void SetTrap(TrapType type) {
    unit.Animator.SetTrap();
    await Task.Delay(1500);

    GameObject prefab = type == TrapType.BearTrap
      ? GameManager.I.bearTrap
      : GameManager.I.spikeTrap;

    Tile tile = unit.CurrentTile;

    GameObject trapObj = Instantiate(prefab, tile.transform);
    trapObj.transform.position = tile.GetPos();
    trapObj.GetComponent<Trap>().Init(unit.Relation, type);
    tile.type = TileType.Trap;

    AfterSkillUse();
  }
}

