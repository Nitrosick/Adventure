using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
  public static BattleManager Instance;

  private UnitData[] allies;
  private UnitData[] enemies;
  private static List<Tile> allySpawns;
  private static List<Tile> enemySpawns;
  public static BattleResult? battleResult;
  public static BattleReward Reward { get; private set; }
  public GameObject corpsePrefab;

  private static readonly float dexterityScaleUnit = 3.5f;
  private static readonly float defaultHitChance = 95f;
  private static readonly float minHitChance = 5f;
  private static readonly float defaultCritChance = 5f;
  private static readonly float minDamage = 0.5f;
  private static readonly float defenseFactor = 10f;

  private void Awake() {
    Instance = this;
    battleResult = null;
    Reward = new BattleReward();
    allies = StateManager.playerUnits.Where(u => u.inSquad).ToArray();
    enemies = StateManager.enemies;

    if (allies == null || allies.Length == 0) {
      Debug.LogError("Ally units not found");
      return;
    }
    if (enemies == null || enemies.Length == 0) {
      Debug.LogError("Enemy units not found");
      return;
    }
    if (corpsePrefab == null) {
      Debug.LogError("BattleManager components initialization error");
      return;
    }

    InitSpawnZones();
    SpawnUnits(allies, allySpawns, UnitRelation.Ally);
    SpawnUnits(enemies, enemySpawns, UnitRelation.Emeny);
    QueueManager.Init();
  }

  private void OnDestroy() {
    allies = null;
    enemies = null;
    allySpawns.Clear();
    enemySpawns.Clear();
    battleResult = null;
    Reward = null;
  }

  private void InitSpawnZones() {
    allySpawns = TileManager.GetSpawns(TileSpawnType.Ally);
    enemySpawns = TileManager.GetSpawns(TileSpawnType.Enemy);
  }

  private void SpawnUnits(UnitData[] unitsData, List<Tile> spawns, UnitRelation relation) {
    Tile focusTile = relation == UnitRelation.Ally
      ? TileManager.allyFocusTile
      : TileManager.enemyFocusTile;

    foreach (UnitData data in unitsData) {
      Tile tile = TileManager.GetRandomFreeTile(spawns);

      if (tile == null) {
        Debug.LogError("Not enough free tiles to place units");
        return;
      }

      Unit unit = StateManager.PrefabDatabase.GetPrefab(data.prefabId);
      if (unit == null) return;

      Vector3 center = new(tile.Coords.x + 0.5f, tile.height, tile.Coords.y + 0.5f);
      Vector3 direction = Vector3.zero;

      if (focusTile != null) {
        direction = (new Vector3(focusTile.Coords.x, 0, focusTile.Coords.y) - center).normalized;
        direction.y = 0;
      }

      unit.transform.position = center;
      unit.FromData(data);
      unit.Init(tile, relation, direction);
      QueueManager.Queue.Add(unit);
    }
  }

  public static float GetHitChance(Unit attacker, Unit target) {
    // FIXME: Расчет шанса попадания
    if (target.Effects.HasEffect("Block")) return 100f;

    float result = defaultHitChance;
    if (attacker.Type == UnitType.Range) {
      if (attacker.Effects.HasEffect("Cover")) result /= 2;
      if (target.Effects.HasEffect("Cover")) result /= 2;
    }

    float dexDelta = attacker.Dexterity - target.Dexterity;
    if (dexDelta < 0) result -= Math.Abs(dexDelta) * dexterityScaleUnit;
    if (result < minHitChance) result = minHitChance;
    return result;
  }

  public static float GetCritModifier(Unit attacker, Unit target) {
    float multiplier = 1f;
    float chance = defaultCritChance;

    float dexDelta = attacker.Dexterity - target.Dexterity;
    if (dexDelta > 0) chance += dexDelta * dexterityScaleUnit;

    bool success = Utils.RollChance(chance);
    // FIXME: Учет предмета во второй руке
    if (success) multiplier = attacker.Equip.primaryWeapon.critModifier;
    return multiplier;
  }

  public static float CalculateDamage(Unit attacker, Unit target) {
    Weapon attackerWeapon = attacker.Equip.primaryWeapon;
    Armor targetArmor = target.Equip.armor;

    float resist = targetArmor.resists[attackerWeapon.damageType];
    float damage = attacker.Equip.GetTotalDamage();
    if (resist != 0) damage *= 1f - (resist / 100f);
    float defense = target.Equip.GetTotalDefense();

    if (attackerWeapon.armorPenetration > 0 && (targetArmor.weight != EquipmentWeight.Light)) {
      defense *= 1f - (attackerWeapon.armorPenetration / 100f);
    }

    float total = damage * Mathf.Exp(-defense / defenseFactor);
    if (target.Effects.HasEffect("Block")) total /= 2;
    return total < minDamage ? minDamage : total;
  }

  public static Effect GetWeaponEffect(Unit attacker, Unit target) {
    Weapon weapon = attacker.Equip.primaryWeapon;
    Armor armor = target.Equip.armor;

    if (weapon.effect == null || weapon.effectChance == 0f) return null;
    float chance = weapon.effectChance;

    if (weapon.effect.effectName == "Bleeding" && armor.weight == EquipmentWeight.Heavy) {
      chance /= 2;
    }

    if (weapon.effect.effectName == "Stun") {
      chance += attacker.Strength - target.Strength;
    }

    if (chance <= 0) return null;
    bool success = Utils.RollChance(chance);
    if (!success) return null;
    return weapon.effect;
  }

  public static void Finish() {
    StateManager.WriteUnitsData(
      QueueManager.Queue
        .Where(unit => unit.Relation == UnitRelation.Ally)
        .ToArray(),
      "allies",
      false
    );

    if (battleResult == BattleResult.Victory) {
      foreach (Unit unit in QueueManager.Queue) {
        if (unit.Relation == UnitRelation.Emeny) Reward.Add(unit.killReward);
      }
    }

    StateManager.battleReward = Reward;
    StateManager.battleResult = battleResult;

    string icon = battleResult == BattleResult.Victory ? "victory" : "defeat";
    string text = battleResult == BattleResult.Victory ? "Victory" : "Defeat";

    SceneController.ShowEventInfo(icon, text);
    SceneController.SwitchScene(StateManager.enterScene);
  }
}
