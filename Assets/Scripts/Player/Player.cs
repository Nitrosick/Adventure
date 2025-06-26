using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
  public static Player Instance;
  private PlayerMove move;
  public PlayerArmy Army { get; private set; }

  public int Gold { get; private set; }
  public int[] Resources { get; private set; } = { 0, 0, 0 };
  public int Villagers { get; private set; }
  public int MaxVillagers { get; private set; }
  public int Experience { get; private set; }
  public int Fame { get; private set; }
  public int Level { get; private set; } = 1;

  private void Awake() {
    Instance = this;
    move = transform.GetComponent<PlayerMove>();
    Army = transform.GetComponent<PlayerArmy>();

    if (move == null || Army == null) {
      Debug.LogError("Player components initialization error");
    }
  }

  private void Start() {
    GetStateData();
  }

  public void ChangeFame(int value) {
    // FIXME: Условия прибавления и отнимания признания
    Fame += value;
  }

  public void AddExpirience(int value) {
    Experience += value;

    while (Experience >= XPForNextLevel) {
      Experience -= XPForNextLevel;
      Level++;
      LevelUp();
    }
  }

  private int XPForNextLevel => GetXPForLevel(Level);

  private int GetXPForLevel(int lvl) {
    float baseXP = 50f;
    float growthFactor = 1.3f;

    float xp = baseXP * Mathf.Pow(lvl, growthFactor);
    return Mathf.RoundToInt(xp / 10f) * 10;
  }

  private void LevelUp() {
    Debug.Log($"{name} повысил уровень до {Level}!");

    // if (Level % 5 == 0) BoostUnitsLevel();
    // FIXME: Повышение уровня юнитов
  }

  private void GetStateData() {
    BattleResult? result = StateManager.battleResult;
    if (result == null) return;

    UnitData[] units = StateManager.allies;
    UnitData[] reserve = StateManager.reserve;
    UnitData[] allUnit = units.Concat(reserve).ToArray();
    Army.UpdateUnits(allUnit);

    if (result == BattleResult.Defeat) {
      transform.position = move.startZone.playerPosition;
      move.CurrentZone = move.startZone;
      Army.UpdateUnitsHPAfterDefeat();
    } else {
      BattleReward reward = StateManager.battleReward;
      if (reward == null) return;
      // FIXME: Слава и предметы из награды
      Gold += reward.Gold;
      for (int i = 0; i < reward.resources.Length; i++) {
        Resources[i] += reward.resources[i];
      }
      AddExpirience(reward.experience);
      MapUI.UpdateResources(Gold, Resources, Villagers, MaxVillagers);
    }
  }
}
