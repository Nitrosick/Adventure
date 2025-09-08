using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
  public static BattlePhase CurrentPhase { get; private set; }

  private void Awake() {
    CurrentPhase = BattlePhase.Movement;
    BattleUI.Instance.SwitchPhase(CurrentPhase);
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

  public async static void NextPhase() {
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

    BattleUI.Instance.SwitchPhase(CurrentPhase);
    await Task.Yield();
    PhaseActions();
  }

  private static void PhaseActions() {
    Unit unit = QueueManager.CurrentUnit;
    List<Skill> skills = unit.Equip.GetActiveSkills();
    if (unit.Relation != UnitRelation.Enemy) BattleUI.Instance.ShowSkills(skills, CurrentPhase, unit);

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        if (unit.Relation == UnitRelation.Enemy) BattleAI.EnemyMove(unit);
        break;

      case BattlePhase.Attack:
        if (
          unit.Effects.HasEffect("Block") ||
          unit.Effects.HasEffect("Wall")
        ) {
          NextPhase();
          return;
        }

        if (unit.Type == UnitType.Range && unit.CurrentProjectiles == 0) {
          // FIXME: Проверка на возможность использовать скиллы
          if (unit.Relation == UnitRelation.Ally) _ = Toast.Show("warning", "No projectiles");
          NextPhase();
          return;
        }

        if (unit.Relation == UnitRelation.Enemy) {
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
