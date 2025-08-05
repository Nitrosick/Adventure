using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MapInput : MonoBehaviour {
  private void Update() {
    if (!Mouse.current.leftButton.wasPressedThisFrame || SceneController.Locked) return;
    HandleClick();
  }

  private async void HandleClick() {
    if (EventSystem.current.IsPointerOverGameObject() || Player.Instance.Move.IsMoving) return;
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    Player player = Player.Instance;

    if (Physics.Raycast(ray, out RaycastHit hit)) {
      string tag = hit.collider.gameObject.tag;

      switch (tag) {
        case "MapZone":
          if (hit.collider.TryGetComponent<MapZone>(out var zone)) {
            List<Vector3> path = player.Move.CalculatePath(zone);
            if (path == null) return;

            _ = CameraController.FocusOn(zone.playerPosition);
            MapUI.HideInteractableButton();

            await player.Move.Move(path);
            player.Move.CurrentZone = zone;
            player.Move.CurrentZone.Visit();
          }
          break;

        case "MapZoneLoot":
          if (hit.collider.TryGetComponent<MapLoot>(out var loot)) {
            if (loot.ParentZone.id != player.Move.CurrentZone.id) return;
            _ = loot.TakeLoot();
          }
          break;
      }
    }
  }
}
