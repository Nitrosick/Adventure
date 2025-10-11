using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Spearman : Unit {
  private readonly float delayAfterAttack = 1.2f;

  private Spearman() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Spearman";
    Description = "The lowest-ranking guard. They usually guard warehouses, storerooms, stables, and if they're lucky, private homes.";
    PrefabId = "u10";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.Spear;
    Level = 2;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 9;
    BehaviorType = AIBehaviorType.TryPierceHit;
  }

  public override async void DealDamage() {
    if (successAttack) {
      int obstacleLayer = LayerMask.NameToLayer("Obstacle");
      int unitLayer = LayerMask.NameToLayer("Unit");
      RaycastHit[] hits = Calculate.HitsOnTrajectory(CurrentTile, Target.CurrentTile);

      foreach (var hit in hits) {
        GameObject go = hit.collider.gameObject;

        if (go.layer == obstacleLayer) {
          Target.Ui.ShowPopup("Obstacle!");
          Target = null;
          PhaseManager.NextPhase();
          return;
        }

        if (go.layer == unitLayer && !DamageBlocked() && go.TryGetComponent<Unit>(out var unit)) {
          float critModifier = Calculate.CritModifier(this, unit);
          float damage = Calculate.Damage(this, unit);
          List<Effect> effects = Calculate.ItemEffects(this, unit);
          foreach (Effect effect in effects) unit.Effects.ApplyEffect(effect);
          unit.Health.TakeDamage(damage, critModifier, false, true);
        }
      }

      await Task.Delay((int)(delayAfterAttack * 1000));
      PhaseManager.NextPhase();
    } else {
      Target.Ui.ShowPopup("Miss!");
    }
    Target = null;
  }
}
