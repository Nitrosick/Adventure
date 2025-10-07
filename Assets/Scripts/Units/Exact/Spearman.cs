using System.Collections.Generic;
using UnityEngine;

public class Spearman : Unit {
  private readonly float attackPointOffset = 0.75f;

  private Spearman() {
    Strength = 4;
    Dexterity = 1;
    Intelligence = 1;

    Name = "Spearman";
    Description = "The lowest-ranking guard. They usually guard warehouses, storerooms, stables, and if they're lucky, private homes.";
    PrefabId = "u10";
    Type = UnitType.Melee;
    AllowedWeapon = EquipmentType.Spear;
    MaxLevel = 6;
    LevelingCoreStat = CoreStat.Strength;
    TotalHealth = 30f;
    MoveSpeed = 3f;
    DefaultMovePoints = 5;
    Initiative = 5;
    Priority = 9;
    BehaviorType = AIBehaviorType.Aggressive; // FIXME: Особое поведение для копейщиков
  }

  public override void DealDamage() {
    if (successAttack) {
      int obstacleLayer = LayerMask.NameToLayer("Obstacle");
      int unitLayer = LayerMask.NameToLayer("Unit");
      int layerMask = LayerMask.GetMask("Unit", "Obstacle");

      Vector3 fixedFrom = CurrentTile.GetPos() + new Vector3(0, attackPointOffset, 0);
      Vector3 fixedTo = Target.CurrentTile.GetPos() + new Vector3(0, attackPointOffset, 0);
      Vector3 direction = (fixedTo - fixedFrom).normalized;
      float distance = Vector3.Distance(fixedFrom, fixedTo);
      RaycastHit[] hits = Physics.RaycastAll(CurrentTile.GetPos(), direction, distance, layerMask);
      System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

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
      PhaseManager.NextPhase();
    } else {
      Target.Ui.ShowPopup("Miss!");
    }
    Target = null;
  }
}
