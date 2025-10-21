using UnityEngine;

[CreateAssetMenu(menuName = "GameObjects/Items/Tool")]
public class Tool : Item {
  public override void Use() {
    MapZone zone = Player.Instance.Move.CurrentZone;
    if (zone == null) return;

    switch (id) {
      case "t1":
        // Кирка
        Fail();
        break;
      case "t2":
        if (
          zone.events.Count > 0 &&
          zone.events[0] == MapZoneType.Excavation &&
          zone.TryGetComponent<MapZoneExcavation>(out var excavation)
        ) {
          Player.Instance.CollectReward(excavation.reward);
          zone.RemoveEvent(MapZoneType.Excavation);
          _ = Toast.Show("success", "You've unearthed a treasure");
        }
        else {
          Fail();
        }
        break;
      case "t3":
        // Кувалда
        Fail();
        break;
      default:
        Fail();
        break;
    }
  }

  private void Fail() {
    _ = Toast.Show("warning", "This tool is useless here");
  }
}
