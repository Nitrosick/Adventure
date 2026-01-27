using System.Collections.Generic;
using UnityEngine;

public class Spearman : UnitCombat {
  private Spearman() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Spearman";
    Description = "The lowest-ranking guard. They usually guard warehouses, storerooms, stables, and if they're lucky, private homes.";
    PrefabId = "u10";
    Type = UnitType.Melee;
    Level = 2;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 9;
    BehaviorType = AIBehaviorType.TryPierceHit;

    AllowedWeapon = new EquipmentType[] {
      EquipmentType.Spear
    };
  }

  public override void DealDamage(bool charged = false) {
    if (successAttack) {
      int obstacleLayer = LayerMask.NameToLayer("Obstacle");
      int unitLayer = LayerMask.NameToLayer("Unit");
      RaycastHit[] hits = Calculate.HitsOnTrajectory(CurrentTile, Target.CurrentTile);

      foreach (var hit in hits) {
        GameObject go = hit.collider.gameObject;

        if (go.layer == obstacleLayer) {
          Target.Ui.ShowPopup("Obstacle!");
          Target = null;
          NextPhase();
          return;
        }

        if (go.layer == unitLayer && !DamageBlocked(charged) && go.TryGetComponent<Unit>(out var unit)) {
          float critModifier = Calculate.CritModifier(this, unit, charged);
          float damage = Calculate.Damage(this, unit, charged);
          List<Effect> effects = Calculate.ItemEffects(this, unit);
          foreach (Effect effect in effects) unit.Effects.ApplyEffect(effect);
          unit.Health.TakeDamage(damage, critModifier);
        }
      }
    } else {
      Target.Ui.ShowPopup("Miss!");
    }
  }
}
