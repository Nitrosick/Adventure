using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Units/Support")]
public class Support : ScriptableObject {
  [Serializable]
  public class LevelDescription {
    public MasteryLevel level;
    [TextArea (3, 20)] public string description;
  }

  [Serializable]
    public class SupportEffects {
    public List<float> values = new () { 0 };
  }

  public string id;
  public string unitName;
  [TextArea(5, 20)] public string description;
  public Sprite icon;
  public SupportBonusType bonusType;
  public SupportPhase phase;
  public List<SupportEffects> effectValues;
  public List<LevelDescription> effectDescriptions = new();

  public string GetEffectDescription(MasteryLevel level) {
    LevelDescription entry = effectDescriptions.Find(e => e.level == level);
    return entry != null ? entry.description : "";
  }
}
