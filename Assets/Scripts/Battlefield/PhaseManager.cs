using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
  public static BattlePhase CurrentPhase { get; private set; }
  private readonly static string[] blockingEffects = new string[] { "Block", "Wall" };

  void Awake() {
    CurrentPhase = BattlePhase.Movement;
    BattleUI.Instance.SwitchPhase(CurrentPhase);
  }

  void OnDestroy() {
    CurrentPhase = BattlePhase.Movement;
  }

  public async static void NextPhase() {
    TileManager.HideGrid();

    // if (QueueManager.Instance.CurrentUnit.IsDead) {
    //   await QueueManager.Instance.NextUnit();
    //   await Task.Yield();
    //   PhaseActions();
    //   return;
    // }

    if (BattleManager.Instance.battleResult != null) return;

    switch (CurrentPhase) {
      case BattlePhase.Movement:
        if (QueueManager.Instance.CurrentUnit.Relation == UnitRelation.Ally)
          _ = Toast.Show("battle", "Attack phase", 1);
        CurrentPhase = BattlePhase.Attack;
        break;

      case BattlePhase.Attack:
        CurrentPhase = BattlePhase.Movement;
        await QueueManager.Instance.NextUnit();
        break;
    }

    BattleUI.Instance.SwitchPhase(CurrentPhase);
    // await Task.Yield();
    PhaseActions();
  }

  private static void PhaseActions() {
    Unit unit = QueueManager.Instance.CurrentUnit;
    List<Skill> skills = unit.Skills.GetActiveSkills();

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
        if (!CanAttack(unit)) {
          NextPhase();
          return;
        }

        if (unit.Relation == UnitRelation.Enemy) {
          BattleAISkills.AttackPhaseSkills(unit);
        } else {
          int targets = TileManager.ShowAttackGrid(unit);
          if (targets == 0 && !unit.Skills.HasNonTargetSkills()) NextPhase();
        }
        break;
    }
  }

  private static bool CanAttack(Unit unit) {
    // TODO: Проверка на возможность использовать скиллы у лучников
    if (unit.Effects.HasAnyEffect(blockingEffects)) return false;
    if (unit.Type == UnitType.Range && unit.CurrentProjectiles == 0) return false;
    if (unit.Relation == UnitRelation.Ally) _ = Toast.Show("warning", "No projectiles");
    return true;
  }
}
