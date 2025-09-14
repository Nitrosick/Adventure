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
      .Where(s => s.data.phase == SupportPhase.BeforeBattle)
      .ToList();
    if (supports.Count == 0) return;
    // FIXME: Действия саппортов до начала боя
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

  private static int LevelIndex(SupportInstance unit) {
    MasteryLevel level = unit.level;
    return Array.IndexOf(Enum.GetValues(level.GetType()), level);
  }

  private static Unit[] GetUnits(UnitRelation relation, bool onlyWounded = false) {
    return QueueManager.Queue
      .Where(u => {
        if (onlyWounded && u.TotalHealth == u.CurrentHealth) return false;
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
