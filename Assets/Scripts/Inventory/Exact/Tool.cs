using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Tool")]
public class Tool : Item {
  public override void Use() {
    MapZone zone = Player.Instance.Move.CurrentZone;
    if (zone == null) return;

    if (
      zone.events.Count > 0 &&
      zone.events[0] == MapZoneType.Excavation &&
      zone.TryGetComponent<MapZoneExcavation>(out var excavation)
    ) {
      // TODO: Можно сделать fade
      switch (id) {
        case "t1": // Pickaxe
          foreach (BlockedPath path in excavation.unlockPathes) path.Unlock();
          zone.RemoveEvent(MapZoneType.Excavation);
          _ = Toast.Show("success", "The blockage has been cleared");
          break;
        case "t2": // Shovel
          Player.Instance.CollectReward(excavation.reward);
          zone.RemoveEvent(MapZoneType.Excavation);
          _ = Toast.Show("success", "You've unearthed a treasure");
          break;
        case "t3": // Sledgehammer

          Fail();
          break;
        default:
          Fail();
          break;
      }
    }
    else {
      Fail();
    }
  }

  private void Fail() {
    _ = Toast.Show("warning", "This tool is useless here");
  }
}
