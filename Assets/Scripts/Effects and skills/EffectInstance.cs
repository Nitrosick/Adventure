public class EffectInstance {
  public Effect effectData;
  public int remainingTurns;
  public float damagePerTurn;

  public EffectInstance(Effect data) {
    effectData = data;
    remainingTurns = data.duration;
    damagePerTurn = data.damage;
  }

  public EffectInstance(Effect data, int duration, float damage) {
    effectData = data;
    remainingTurns = duration;
    damagePerTurn = damage;
  }

  public void Tick(Unit target) {
    if (effectData.damage > 0) {
      target.TakeDamage(effectData.damage, 1f, true);
    }
    remainingTurns--;
  }

  public bool IsExpired => remainingTurns <= 0;
}
