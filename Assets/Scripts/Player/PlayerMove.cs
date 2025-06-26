using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
  private readonly float moveSpeed = 3f;

  private bool isMoving = false;

  public MapZone startZone;
  public MapZone CurrentZone { get; set; }

  private LayerMask zoneLayer;
  private Camera mainCamera;
  private PlayerAnimator animator;

  private void Awake() {
    zoneLayer = LayerMask.GetMask("MapZone");
    mainCamera = Camera.main;
    animator = transform.GetComponent<PlayerAnimator>();

    if (zoneLayer < 1 || mainCamera == null || animator == null) {
      Debug.LogError("Player movement components initialization error");
      return;
    }

    if (StateManager.currentPlayerZoneId > 0) {
      CurrentZone = MapZoneManager.FindById(StateManager.currentPlayerZoneId);
      transform.position = CurrentZone.playerPosition;
    }
    else {
      CurrentZone = startZone;
    }
  }

  private void Start() {
    _ = CameraController.FocusOn(transform.position, true);
  }

  private void Update() {
    if (isMoving || SceneController.Locked) return;
    if (Mouse.current.leftButton.wasPressedThisFrame) DetectZone();
  }

  private async void DetectZone() {
    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, zoneLayer)) {
      MapZone target = hit.collider.GetComponent<MapZone>();

      List<Vector3> path = CalculatePath(target);
      if (path == null) return;

      _ = CameraController.FocusOn(target.playerPosition);

      await Move(path);
      CurrentZone = target;
      StateManager.currentPlayerZoneId = target.id;
      CurrentZone.GetComponent<MapZoneEvent>().CheckEvents();
    }
  }

  private List<Vector3> CalculatePath(MapZone target) {
    Way[] pathes = CurrentZone.GetComponentsInChildren<Way>();
    if (pathes == null || pathes.Length == 0) return null;

    foreach (var path in pathes) {
      if (path.id == target.id) {
        return new List<Vector3>(path.waypoints) {
          target.playerPosition
        };
      }
    }

    return null;
  }

  private async Task Move(List<Vector3> path) {
    if (path == null || path.Count == 0) return;

    isMoving = true;
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
      } else {
        transform.position = target;
      }
    }

    isMoving = false;
    animator.SetMoving(false);
  }
}
