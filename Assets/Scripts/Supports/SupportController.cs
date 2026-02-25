using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class SupportController {
  private static List<SupportInstance> allSupports = new();
  private static bool enabled = false;

  public static void Init(List<SupportInstance> list) {
    if (list.Count == 0) return;
    enabled = true;
    allSupports = list;
    BeforeBattle();
  }

  private static void BeforeBattle() {
    List<SupportInstance> supports = allSupports
      .Where(s => s.data.phase == SupportPhase.BeforeBattle || s.data.phase == SupportPhase.Global)
      .ToList();
    if (supports.Count == 0) return;

    foreach (SupportInstance sup in supports) {
      int i = LevelIndex(sup);

      switch (sup.data.id) {
        case "su2":
          TileManager.HighlightChests();
          break;
        case "su3":
          TileManager.UncoverTraps(sup.data.effectValues[i].values[0]);
          break;
      }
    }
  }

  public static async Task EveryTurn() {
    if (!enabled) return;
    List<SupportInstance> supports = allSupports
      .Where(s => s.data.phase == SupportPhase.EveryTurn)
      .ToList();
    if (supports.Count == 0) return;

    foreach (SupportInstance sup in supports) {
      int i = LevelIndex(sup);
      Unit[] affectedUnits = GetUnits(sup.relation, true);

      switch (sup.data.id) {
        case "su1":
          if (i < 2) {
            Unit random = Randomiser.GetRandomUnit(affectedUnits);
            if (random == null) return;
            _ = CameraController.FocusOn(random.transform.position);
            random.Health.Heal(sup.data.effectValues[i].values[0]);
          } else {
            foreach (Unit unit in affectedUnits) {
              unit.Health.Heal(sup.data.effectValues[i].values[0]);
            }
          }
          break;
      }
    }

    // TODO: Фокус на палатку для саппортов
    _ = Toast.Show("heal", "Supports phase", 1);
    await Task.Delay(1500);
  }

  public static float[] GetBonus(
    string id,
    bool inBattle = true,
    UnitRelation relation = UnitRelation.Ally,
    Unit unit = null
  ) {
    SupportInstance sup = null;

    if (inBattle) {
      sup = allSupports
        .Where(s => s.relation == relation)
        .FirstOrDefault(s => s.data.id == id);
    }
    else {
      sup = Player.Instance.Army.Supports
        .FirstOrDefault(s => s.data.id == id && s.inSquad);
    }

    if (sup == null) return new float[] { 0, 0 };
    int level = LevelIndex(sup);
    float[] values = sup.data.effectValues[level].values.ToArray();

    if (unit != null) {
      if (id == "su6") {
        if (level < 4 && !unit.IsHero) return new float[] { 0, values[1] };
        return values;
      }
    }

    return values;
  }

  private static int LevelIndex(SupportInstance unit) {
    MasteryLevel level = unit.level;
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }

  private static Unit[] GetUnits(UnitRelation relation, bool onlyWounded = false) {
    return QueueManager.Instance.Queue
      .Where(u => {
        if (u.IsDead || (onlyWounded && u.Health.GetMaxHP() == u.CurrentHealth)) return false;
        return u.Relation == relation;
      })
      .ToArray();
  }
}
