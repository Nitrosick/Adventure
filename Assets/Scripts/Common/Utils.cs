using UnityEngine;

public static class Utils
{
  public static bool RollChance(float chance) {
    return Random.Range(0f, 100f) < chance;
  }

  public static int GetRandomInRange(int min, int max) {
    return Random.Range(min, max + 1);
  }
}
