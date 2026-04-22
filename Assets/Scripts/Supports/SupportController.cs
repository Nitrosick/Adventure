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

  private static string G(string text) => Utils.GreyText(text);

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
            Unit random = Randomiser.GetRandomUnit(affectedUnits);
            if (random == null) return;

            float value = sup.data.effectValues[i].values[0];
            random.Health.Heal(value);
            LogUI.Instance.Add($"{sup.data.unitName} {G("heals")} {random.Name} {G("by")} {value} {G("points")}");
          } else {
            if (affectedUnits.Count() == 0) return;

            float value = sup.data.effectValues[i].values[0];
            foreach (Unit unit in affectedUnits) unit.Health.Heal(value);
            LogUI.Instance.Add($"{sup.data.unitName} {G("heals")} {affectedUnits.Count()} {G("units by")} {value} {G("points")}");
          }
          break;
      }
    }
  }

  public static float[] GetBonus(
    string id,
    bool inBattle = true,
    UnitRelation relation = UnitRelation.Ally,
    Unit targetUnit = null
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

    if (targetUnit != null) {
      if (id == "su6") {
        if (level < 4 && !targetUnit.IsHero) return new float[] { 0, values[1] };
        return values;
      }
    }

    return values;
  }

  public static int LevelIndex(SupportInstance unit) {
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
