using System;
using System.Collections.Generic;
using System.Linq;

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
          TileManager.UncoverTraps(sup.data.effectValues[i]);
          break;
      }
    }
  }

  public static void EveryTurn() {
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
            Unit random = GetRandomUnit(affectedUnits);
            if (random != null) random.Health.Heal(sup.data.effectValues[i]);
          } else {
            foreach (var unit in affectedUnits) {
              unit.Health.Heal(sup.data.effectValues[i]);
            }
          }
          break;
      }
    }
  }

  public static float GetBonus(
    string id,
    bool inBattle = true,
    UnitRelation relation = UnitRelation.Ally
  ) {
    SupportInstance sup = null;

    if (inBattle) {
      sup = allSupports
        .Where(s => s.relation == relation)
        .FirstOrDefault(s => s.data.id == id);
    } else {
      sup = Player.Instance.Army.Supports
        .FirstOrDefault(s => s.data.id == id && s.inSquad);
    }

    if (sup == null) return 0;
    return sup.data.effectValues[LevelIndex(sup)];
  }

  private static int LevelIndex(SupportInstance unit) {
    MasteryLevel level = unit.level;
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }

  private static Unit[] GetUnits(UnitRelation relation, bool onlyWounded = false) {
    return QueueManager.Queue
      .Where(u => {
        if (u.IsDead || (onlyWounded && u.Health.GetMaxHP() == u.CurrentHealth)) return false;
        return u.Relation == relation;
      })
      .ToArray();
  }

  private static Unit GetRandomUnit(Unit[] units) {
    if (units.Length == 0) return null;
    int i = UnityEngine.Random.Range(0, units.Length);
    return units[i];
  }
}
