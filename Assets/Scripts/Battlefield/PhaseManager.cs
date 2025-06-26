using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
  public static BattlePhase CurrentPhase { get; private set; }

  private void Awake() {
    CurrentPhase = BattlePhase.Movement;
    BattleUI.SwitchPhase(CurrentPhase);
  }

  private void OnDestroy() {
    CurrentPhase = BattlePhase.Movement;
  }

  private static void PhasePreSwitch() {
    QueueManager.CheckBattleIsOver();
    TileManager.HideGrid();

    if (QueueManager.CurrentUnit.IsDead) {
      QueueManager.NextUnit();
      return;
    }

    QueueManager.CurrentUnit.ResetMovePoints();
  }

  public static void NextPhase() {
    PhasePreSwitch();

    if (BattleManager.battleResult != null) return;

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        CurrentPhase = BattlePhase.Attack;
        break;

      case BattlePhase.Attack:
        CurrentPhase = BattlePhase.Movement;
        QueueManager.NextUnit();
        break;
    }

    BattleUI.SwitchPhase(CurrentPhase);
    PhaseActions();
  }

  private static void PhaseActions() {
    Unit unit = QueueManager.CurrentUnit;
    List<Skill> skills = unit.Equip.GetSkills();
    if (unit.Relation != UnitRelation.Emeny) BattleUI.ShowSkills(skills, CurrentPhase, unit);

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        if (unit.Relation == UnitRelation.Emeny) BattleAI.EnemyMove(unit);
        break;

      case BattlePhase.Attack:
        if (unit.Effects.HasEffect("Block")) {
          NextPhase();
          return;
        }

        if (unit.Relation == UnitRelation.Emeny) {
          if (unit.Target != null) unit.OnAttack();
          else NextPhase();
        } else {
          int targets = TileManager.ShowAttackGrid(unit);
          // FIXME: Сделать проверку для юнитов, которые могут атаковать по площади
          if (targets == 0 && !unit.Equip.HasAttackPhaseSkills()) NextPhase();
        }
        break;
    }
  }
}
