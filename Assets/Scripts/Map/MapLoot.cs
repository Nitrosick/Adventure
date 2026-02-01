using System.Threading.Tasks;
using UnityEngine;

public class MapLoot : MonoBehaviour {
  public string id;
  public MapZone ParentZone { get; private set; }
  public Reward reward;
  public ParticleSystem pickEffect;

  void Awake() {
    ParentZone = GetComponentInParent<MapZone>();

    if (ParentZone == null) {
      Debug.LogError("Map loot components initialization error");
    }
  }

  public async Task TakeLoot() {
    if (pickEffect == null) return;
    Instantiate(pickEffect, transform.position, Quaternion.identity);

    Player.Instance.CollectReward(reward);
    StateManager.collectedZoneLoot.Add(id);
    _ = Toast.Show("success", "Loot picked up");

    await Task.Yield();
    Destroy(gameObject);
    TooltipManager.Instance.HideTooltip();
  }
}
