using UnityEngine;

public class MapZoneCollecting : MonoBehaviour {
  private MapZone zone;
  public Reward reward;
  public int CollectedAt { get; set; }
  public int respawn = 1;
  public bool onetime;

  void Awake() {
    zone = transform.GetComponent<MapZone>();

    if (zone == null) {
      Debug.LogError("Map zone collecting components initialization error");
    }
  }

  public void OpenCollectingPanel() {
    CollectingModalUI.Instance.Confirmation(CollectItems, reward);
  }

  private void CollectItems(bool accepted) {
    if (!accepted) return;

    CollectedAt = StateManager.globalTicks;
    Player.Instance.CollectReward(reward);
    _ = Toast.Show("success", "Items collected");

    if (onetime) zone.RemoveEvent(MapZoneType.Collecting);
    else StateManager.zonesState[zone.id].collectedAt = CollectedAt;

    zone.SwitchIcon(false);
    zone.SwitchInteractiveObjects();
    transform.GetComponent<MapZoneEvent>().CheckEvents();
  }
}
