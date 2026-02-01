using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
  private readonly float moveSpeed = 3f;
  public bool IsMoving { get; private set; }

  public MapZone startZone;
  public MapZone CurrentZone { get; set; }
  private LayerMask zoneLayer;
  private Camera mainCamera;
  private PlayerAnimator animator;

  void Awake() {
    zoneLayer = LayerMask.GetMask("MapZone");
    mainCamera = Camera.main;
    animator = transform.GetComponent<PlayerAnimator>();

    if (zoneLayer < 1 || mainCamera == null || animator == null) {
      Debug.LogError("Player movement components initialization error");
      return;
    }

    if (int.Parse(StateManager.startPlayerZoneId) > 0) {
      startZone = MapZoneManager.FindById(StateManager.startPlayerZoneId);
    }

    if (int.Parse(StateManager.currentPlayerZoneId) > 0) {
      CurrentZone = MapZoneManager.FindById(StateManager.currentPlayerZoneId);
      SetPlayerPosition(CurrentZone);
    } else {
      CurrentZone = startZone;
    }

    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    if (state.Count == 0) {
      state[CurrentZone.id] = new MapZoneData {
        events = CurrentZone.events,
        visited = true
      };
      CurrentZone.ShowPathLines();
    }
  }

  void Start() {
    _ = CameraController.FocusOn(transform.position, true);
  }

  public List<Vector3> CalculatePath(MapZone target) {
    Way[] pathes = CurrentZone.GetComponentsInChildren<Way>();
    if (pathes == null || pathes.Length == 0) return null;

    foreach (var path in pathes) {
      if (path.id == target.id) {
        if (path.blocked) {
          _ = Toast.Show("warning", "Passage is blocked");
          return null;
        }

        return new List<Vector3>(path.waypoints) {
          target.playerPosition
        };
      }
    }

    return null;
  }

  public void SetPlayerPosition(MapZone zone) {
    transform.position = zone.playerPosition;
    CurrentZone = zone;
    StateManager.currentPlayerZoneId = zone.id;
    _ = CameraController.FocusOn(transform.position, true);
  }

  public async Task Move(List<Vector3> path) {
    if (path == null || path.Count == 0) return;

    IsMoving = true;
    animator.SetMoving(true);

    for (int i = 0; i < path.Count; i++) {
      Vector3 startPosition = transform.position;
      Vector3 target = path[i];

      float distance = Vector3.Distance(startPosition, target);
      float duration = distance / moveSpeed;
      float elapsed = 0f;

      Vector3 direction = (target - startPosition).normalized;

      if (direction != Vector3.zero) {
        while (elapsed < duration) {
          elapsed += Time.deltaTime;

          transform.position = Vector3.Lerp(startPosition, target, elapsed / duration);
          animator.RotateTowards(direction);

          await Task.Yield();
        }
      }
      else {
        transform.position = target;
      }
    }

    IsMoving = false;
    animator.SetMoving(false);
  }
}
