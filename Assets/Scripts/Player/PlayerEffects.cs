using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEffects : MonoBehaviour {
  private readonly List<Buff> buffs = new ();

  void Start() {
    foreach (string buffId in StateManager.playerBuffs) AddBuff(buffId, true);
  }

  public bool HasBuff(string buffId) {
    return buffs.Any(b => b.id == buffId);
  }

  public void AddBuff(string buffId, bool init = false) {
    if (HasBuff(buffId)) return;
    Buff buff = Factory.CreateBuffById(buffId);
    if (buff == null) return;
    buffs.Add(buff);
    UpdateUI();
    if (!init) UpdateState();
  }

  public void RemoveBuff(string buffId) {
    buffs.RemoveAll(b => b.id == buffId);
    UpdateUI();
    UpdateState();
  }

  void UpdateUI() {
    MapUI.Instance.UpdateBuffs(buffs);
  }

  void UpdateState() {
    StateManager.playerBuffs = buffs
      .Select(b => b.id)
      .ToHashSet();
  }
}
