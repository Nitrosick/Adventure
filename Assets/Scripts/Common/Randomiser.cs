public static class Randomiser {
  public static bool RollChance(float chance) {
    if (chance >= 100f) return true;
    if (chance <= 0f) return false;
    return UnityEngine.Random.Range(0f, 100f) < chance;
  }

  public static int GetRandomInRange(int min, int max) {
    return UnityEngine.Random.Range(min, max + 1);
  }

  public static Unit GetRandomUnit(Unit[] units) {
    if (units.Length == 0) return null;
    int i = UnityEngine.Random.Range(0, units.Length);
    return units[i];
  }
}
