using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UnitHealth : MonoBehaviour {
  private Unit unit;
  protected readonly int objectDestroyTime = 10;

  private void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit health components initialization error");
    }
  }

  public float GetMaxHP() {
    if (unit.IsHero) return unit.TotalHealth + AbilityController.HealthBonus();
    return unit.TotalHealth;
  }

  public void TakeDamage(float damage, float modifier, bool isTickDamage = false) {
    float totalDamage = damage * modifier;

    if (modifier > 1f) {
      unit.Ui.ShowPopup(totalDamage.ToString(), PopupType.Crit);
      if (!isTickDamage) _ = CameraController.Shake(1.2f);
    }
    else {
      unit.Ui.ShowPopup(totalDamage.ToString(), PopupType.Negative);
      if (!isTickDamage) _ = CameraController.Shake(0.8f);
    }

    if (unit.BehaviorType == AIBehaviorType.HoldPosition) {
      unit.BehaviorType = AIBehaviorType.KeepDistance;
    }

    if (totalDamage >= GetMaxHP()) TriggerAchievement("ac2");

    if (totalDamage >= unit.CurrentHealth) {
      unit.CurrentHealth = 0;
      Die();
    }
    else {
      unit.CurrentHealth -= totalDamage;
      unit.Ui.UpdateHealth(GetMaxHP(), unit.CurrentHealth);

      if (!isTickDamage) {
        if (unit.Effects.HasAnyEffect(new string[] { "Stun", "Root" })) unit.FinishAction();
        else unit.Animator.TakeDamage();
      }
    }
  }

  private void Die() {
    // Bandit ids
    if (new string[] { "u3", "u6", "u7" }.Contains(unit.PrefabId)) TriggerAchievement("ac1");

    unit.IsDead = true;
    unit.CurrentTile.OccupiedBy = null;
    unit.UnitCollider.enabled = false;
    unit.Ui.ClearMarkers();
    unit.Ui.HideHealthBar();
    unit.Ui.HideChargesBar();
    unit.Effects.ClearEffects();
    unit.Animator.Die();
    _ = MakeCorpse();
  }

  public void Heal(float _value, bool inBattle = true) {
    if (unit.CurrentHealth == GetMaxHP()) return;
    float value = _value;
    if (inBattle && unit.Relation == UnitRelation.Ally) value *= AbilityController.HealBonus();
    unit.CurrentHealth += value;
    if (unit.CurrentHealth > GetMaxHP()) unit.CurrentHealth = GetMaxHP();

    if (!inBattle) {
      Player.Instance.Army.UpdateState();
    }
    else {
      unit.Ui.ShowPopup(value.ToString(), PopupType.Positive);
      unit.Ui.UpdateHealth(GetMaxHP(), unit.CurrentHealth);
      if (BattleManager.Instance.healEffect != null) {
        ParticleSystem effect = Instantiate(
          BattleManager.Instance.healEffect,
          unit.transform.position + new Vector3(0, 0.3f, 0),
          Quaternion.identity
        );
        Destroy(effect.gameObject, 2);
      }
    }
  }

  private async Task MakeCorpse() {
    await Task.Delay(objectDestroyTime * 1000);

    GameObject model = transform.Find("Model").gameObject;
    Destroy(model);

    Instantiate(
      BattleManager.Instance.corpsePrefab,
      transform.position,
      Quaternion.Euler(0, 65, 0)
    );
  }

  private void TriggerAchievement(string id, float value = 1f) {
    if (unit.Relation != UnitRelation.Enemy) return;
    AchievementManager.UpdateAchievement(id, value, true);
  }
}
