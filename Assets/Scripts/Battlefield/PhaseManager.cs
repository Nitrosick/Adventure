using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
  public static BattlePhase CurrentPhase { get; private set; }

  void Awake() {
    CurrentPhase = BattlePhase.Movement;
    BattleUI.Instance.SwitchPhase(CurrentPhase);
  }

  void OnDestroy() {
    CurrentPhase = BattlePhase.Movement;
  }

  public async static void NextPhase() {
    TileManager.HideGrid();

    if (QueueManager.Instance.CurrentUnit.IsDead) {
      QueueManager.Instance.NextUnit();
      return;
    }

    if (BattleManager.battleResult != null) return;

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        if (QueueManager.Instance.CurrentUnit.Relation == UnitRelation.Ally)
          _ = Toast.Show("battle", "Attack phase", 1);
        CurrentPhase = BattlePhase.Attack;
        break;

      case BattlePhase.Attack:
        CurrentPhase = BattlePhase.Movement;
        QueueManager.Instance.NextUnit();
        break;
    }

    BattleUI.Instance.SwitchPhase(CurrentPhase);
    await Task.Yield();
    PhaseActions();
  }

  private static void PhaseActions() {
    Unit unit = QueueManager.Instance.CurrentUnit;
    List<Skill> skills = unit.Equip.GetActiveSkills();

    if (unit.Relation != UnitRelation.Enemy) {
      BattleUI.Instance.ShowSkills(skills, CurrentPhase, unit);
    } else {
      BattleAI.Init(unit);
    }

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        if (unit.Relation == UnitRelation.Enemy) BattleAI.EnemyMove();
        break;

      case BattlePhase.Attack:
        if (unit.Effects.HasAnyEffect(new string[] { "Block", "Wall" })) {
          NextPhase();
          return;
        }

        if (unit.Type == UnitType.Range && unit.CurrentProjectiles == 0) {
          // TODO: Проверка на возможность использовать скиллы у лучников
          if (unit.Relation == UnitRelation.Ally) _ = Toast.Show("warning", "No projectiles");
          NextPhase();
          return;
        }

        if (unit.Relation == UnitRelation.Enemy) {
          BattleAISkills.AttackPhaseSkills(unit);
        } else {
          int targets = TileManager.ShowAttackGrid(unit);
          if (targets == 0 && !unit.Equip.HasNonTargetSkills()) NextPhase();
        }
        break;
    }
  }
}
