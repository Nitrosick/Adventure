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
public enum TileType {
  Open,
  Obstacle,
  Cover,
  Tree,
  Breakable,
  Loot
}

public enum TileSpawnType {
  No,
  Ally,
  Enemy
}

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

public enum SkillName {
  Block
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
  Emeny,
  Neutral
}

public enum UnitEquipSlot {
  Primary,
  Secondary,
  Armor
}

public enum MasteryLevel {
  Novice,
  Apprentice,
  Adept,
  Expert,
  Master
}

// Items
public enum EquipmentType {
  OneHandWeapon,
  TwoHandWeapon,
  Crossbow,
  Bow,
  Shield,
  Armor
}

public enum EquipmentWeight {
  Light,
  Medium,
  Heavy
}

public enum EquipmentRarity {
  Common,
  Rare,
  Epic,
  Legendary
}

// Map
[JsonConverter(typeof(StringEnumConverter))]
public enum MapZoneType {
  Home,
  InstantBattle,
  Guard,
  Ambush,
  Constructing,
  Recruitment
  // ...
}

public enum MapZoneFeature {
  Healing
}
