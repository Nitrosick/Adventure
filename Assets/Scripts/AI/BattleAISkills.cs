using System.Collections.Generic;
using System.Linq;

public static class BattleAISkills {
  public static void AttackPhaseSkills(Unit unit) {
    List<Unit> players = BattleAI.PlayerUnits();
    Unit target = unit.Target;
    bool skip = true;

    foreach (Skill skill in unit.Skills.GetActiveSkills().Where(s => s.skillPhases.Contains(BattlePhase.Attack))) {
      if (unit.SkillCharges < skill.cost) continue;

      switch (skill.skillName) {
        case "Charged attack":
          if (target == null) break;

          if (
            target.CurrentHealth < target.Health.GetMaxHP() ||
            target.Effects.HasAnyEffect(new[] { "Stun", "Root" })
          ) {
            skip = false;
            unit.SetSkillCharges(-skill.cost);
            unit.SetAttackType(AttackType.Charged);
          }
          break;

        case "Fan attack":
          if (target == null) break;

          var neighbors = unit.CurrentTile.Neighbors;
          int n = neighbors.Count;

          for (int i = 0; i < n; i++) {
            if (neighbors[i] != target.CurrentTile) continue;

            Unit l = neighbors[(i - 1 + n) % n].OccupiedBy;
            Unit r = neighbors[(i + 1) % n].OccupiedBy;

            if (
              l?.Relation == UnitRelation.Enemy ||
              r?.Relation == UnitRelation.Enemy
            ) break;

            if (
              l?.Relation == UnitRelation.Ally ||
              r?.Relation == UnitRelation.Ally
            ) {
              skip = false;
              unit.SetSkillCharges(-skill.cost);
              unit.SetAttackType(AttackType.Fan);
            }
            break;
          }
          break;

        case "Block":
        case "Wall":
          if (target != null) break;

          if (
            BattleAIHeplers.PlayerHasShooters(players) ||
            BattleAIHeplers.CountEnemiesInRange(players, unit, 4f) > 1
          ) {
            skip = false;
            unit.SetSkillCharges(-skill.cost);
            unit.Skills.BlockStance(skill.skillName == "Wall" ? "e2" : "e7");
          }
          break;
      }
    }

    if (target != null) unit.OnAttack();
    else if (skip) PhaseManager.NextPhase();
  }

  public static void MovePhaseSkills(Unit unit) {
    // if (enemy.SkillCharges == 0) return;

    // List<Skill> skills = enemy.Skills.GetActiveSkills()
    //   .Where(s => s.skillPhases.Contains(BattlePhase.Movement))
    //   .ToList();
    // if (skills.Count == 0) return;
  }
}
