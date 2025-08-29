public class EffectInstance {
  public Effect effectData;
  public int remainingTurns;
  public float damagePerTurn;

  public EffectInstance(Effect data, int duration = 0, float damage = 0) {
    effectData = data;
    remainingTurns = duration == 0 ? data.duration : duration;
    damagePerTurn = damage == 0 ? data.damage : damage;
  }

  public void Tick(Unit target) {
    if (effectData.damage > 0) {
      target.Health.TakeDamage(effectData.damage, 1f, true);
    }
    remainingTurns--;
  }

  public bool IsExpired => remainingTurns <= 0;
}
