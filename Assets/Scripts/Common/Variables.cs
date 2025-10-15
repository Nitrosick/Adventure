/*
Font
----
Button: Height 75 / Size 30 / Color #FFFFFF / Grenze-Bold / Pixels per unit 3.5
Label Button: Height 75 / Size 30 / Color #111110 / Grenze-Bold / Pixels per unit 3.5
Event: Size 45 / Color #111110 / Grenze-Bold
Title: Size 30 / Grenze-Bold / Letter spacing -5
Text: Size 24 / Oswald / Letter spacing -5
Parameter key: Size 24 / Oswald / Letter spacing -5 / Opacity 200

Common colors
-------------
Main blue: #2B8EF3 / #174E87
Negative: #F61010 / #781010
Positive: #81D11F
Warning: #EFBF0D
Inactive: #A0A0A0

Rarity colors
-------------
Novice: #A0A0A0
Apprentice: #618C2D
Adept: #306DAB
Expert: #6948A4
Master: #CF8F0B
Key: #A7E7E4

Bronze: #C2771D
Silver: #CDCDCE
Gold: #E2B63F

Common
------
Gap: 30
Screen padding: 75
Panel padding: 25
Panel gap: 15
Elements shadow: 0 / -3

Icons
-----
Default size: 25x25
Event size: 30x30
Color: #4B4A47

Scrollbars
----------
Width: 15
Default: #5C523F
Highlighted / Pressed / Selected: #938569
Disabled: #5C523F o-128
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// Battle
public enum BattlePhase {
  Movement,
  Attack,
  Ability
}

public enum BattleResult {
  Victory,
  Defeat,
  Retreat
}

public enum DamageType {
  No,
  Slash,
  Pierce,
  Chop,
  Crash,
  Blood
}

public enum AIBehaviorType {
  Aggressive,
  PriorityTarget,
  KeepDistance,
  Retreat,
  Passive,
  HoldPosition,
  TryPierceHit
}

public enum ShotTrajectory {
  No,
  Direct,
  Arc,
  FromAbove
}

public enum TrapType {
  BearTrap,
  Spikes
}

// Battlefield tiles
public enum TileType {
  Open,
  Obstacle,
  Cover,
  Tree,
  Breakable,
  Loot,
  Climb,
  Trap
}

public enum TileSpawnType {
  No,
  AnyAlly,
  AnyEnemy,
  AllyShooter,
  EnemyShooter,
  Boss,
  Reinforcement
}

// UI
public enum PopupType {
  Negative,
  Positive,
  Crit,
  Neutral,
  Inactive
}

// Units
public enum CoreStat {
  Strength,
  Dexterity,
  Intelligence
}

public enum UnitType {
  Melee,
  Range,
  Mage,
  Siege
}

public enum UnitRelation {
  Ally,
  Enemy,
  Neutral
}

public enum UnitEquipSlot {
  Primary,
  Secondary,
  Armor,
  Additional
}

public enum MasteryLevel {
  Novice,
  Apprentice,
  Adept,
  Expert,
  Master
}

public enum SupportBonusType {
  Healing
}

public enum SupportPhase {
  BeforeBattle,
  EveryTurn
}

// Items
public enum EquipmentType {
  OneHandWeapon,
  TwoHandWeapon,
  Crossbow,
  Bow,
  Shield,
  Armor,
  TowerShield,
  Additional,
  Spear
}

public enum EquipmentWeight {
  Light,
  Medium,
  Heavy
}

public enum Rarity {
  Common,
  Rare,
  Epic,
  Legendary,
  Relic,
  Key
}

public enum ItemBonus {
  Projectiles
}

// Map
[JsonConverter(typeof(StringEnumConverter))]
public enum MapZoneType {
  Home,
  Battle,
  Constructing,
  Recruitment,
  Ambush,
  Quest,
  Collecting,
  Task
}

public enum MapZoneFeature {
  Healing,
  Trading,
  Weaponsmith,
  Armorer,
  Training,
  Quests
}

public enum Building {
  Watchtower,
  Lumbercamp
}

// Quests and achievements
public enum QuestState {
  Inactive,
  Accepted,
  Completed
}

public enum QuestObjective {
  Fight,
  VisitZone,
  GetItem,
  BringItem
}

// Almanac
public enum KnowledgeSection {
  Common,
  AdventureMap,
  Battlefield,
  Lore,
  Player
}

// Skills and effects
public enum SkillName {
  Block,
  Parry,
  Wall,
  Comfort
}

public enum AbilityLevel {
  No,
  Bronze,
  Silver,
  Gold
}

public enum AbilityBonusType {
  Damage,
  Block,
  Evasion,
  Healing,
  Resist,
  Priority,
  AmbushProtect,
  Experience,
  Prices,
  Crit,
  Requirements,
  Precision,
  Skills,
  Health,
  Fame,
  Movement
}

// Filters
public enum MenuFilter {
  All,
  AllUnits,
  FreeUnits,
  UnitsInSquad,
  AllSupports,
  FreeSupports,
  SupportsInSquad,
  AllEquipment,
  Weapon,
  Armor,
  Additional,
  AllItems,
  Medicine,
  Leveling,
  Goods,
  Key
  // FIXME: Добавить фильтры по типам юнитов
}
