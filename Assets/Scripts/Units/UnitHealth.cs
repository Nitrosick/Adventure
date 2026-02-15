using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UnitHealth : MonoBehaviour {
  private Unit unit;
  protected readonly int objectDestroyTime = 10;

  void Awake() {
    unit = transform.GetComponent<Unit>();

    if (unit == null) {
      Debug.LogError("Unit health components initialization error");
    }
  }

  public float GetMaxHP() {
    if (unit.IsHero) return unit.TotalHealth + AbilityController.HealthBonus();
    return unit.TotalHealth;
  }

  public void TakeDamage(
    float damage,
    float modifier,
    bool tickDamage = false,
    bool charged = false
  ) {
    float totalDamage = damage * modifier;
    bool isCrit = modifier > 1f || charged;

    unit.Ui.ShowPopup(totalDamage.ToString(), isCrit ? PopupType.Crit : PopupType.Negative);
    if (!tickDamage) _ = CameraController.Shake(isCrit ? 1.2f : 0.8f);

    if (unit.BehaviorType == AIBehaviorType.HoldPosition) {
      unit.BehaviorType = AIBehaviorType.KeepDistance;
    }

    if (totalDamage >= GetMaxHP()) TriggerAchievement("ac2");

    if (totalDamage >= unit.CurrentHealth) {
      unit.CurrentHealth = 0;
      Die();
    } else {
      unit.CurrentHealth -= totalDamage;
      unit.Ui.UpdateHealth(GetMaxHP(), unit.CurrentHealth);
      if (!tickDamage) unit.Animator.TakeDamage();
    }
  }

  private void Die() {
    // FIXME: Добавить все id бандитских юнитов
    // Bandit ids
    if (new string[] { "u3", "u6", "u7", "u13" }.Contains(unit.PrefabId)) TriggerAchievement("ac1");

    unit.IsDead = true;
    unit.CurrentTile.OccupiedBy = null;
    unit.UnitCollider.enabled = false;
    unit.Ui.ClearMarkers();
    unit.Ui.HideHealthBar();
    unit.Ui.HideChargesBar();
    unit.Effects.ClearEffects();
    unit.Animator.Die();

    if (!QueueManager.CheckBattleIsOver()) _ = MakeCorpse();
  }

  public void Heal(float _value, bool inBattle = true) {
    if (unit.CurrentHealth == GetMaxHP()) return;
    float value = _value;

    if (value != -1) {
      if (inBattle && unit.Relation == UnitRelation.Ally) value *= AbilityController.HealBonus();
      unit.CurrentHealth += value;
      if (unit.CurrentHealth > GetMaxHP()) unit.CurrentHealth = GetMaxHP();
    }
    else unit.CurrentHealth = GetMaxHP();

    if (!inBattle) {
      Player.Instance.Army.UpdateState();
    } else {
      if (value > 0) unit.Ui.ShowPopup(value.ToString(), PopupType.Positive);
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
