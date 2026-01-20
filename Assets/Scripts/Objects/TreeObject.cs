using UnityEngine;
using UnityEngine.EventSystems;

public class TreeObject : MonoBehaviour {
  public Tile ParentTile { get; private set; }
  protected readonly int objectDestroyTime = 5;

  private void Awake() {
    ParentTile = transform.GetComponentInParent<Tile>();

    if (ParentTile == null) {
      Debug.LogError("Tree object components initialization error");
    }
  }

  void OnMouseEnter() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    ParentTile.Hover();
  }

  void OnMouseExit() {
    ParentTile.Unhover();
  }

  public void Chop() {
    foreach (Transform obj in transform) {
      if (!obj.TryGetComponent<Animator>(out var animator)) continue;
      animator.SetTrigger("Fall");
    }

    transform.GetComponent<BoxCollider>().enabled = false;
    ParentTile.type = TileType.Open;
    _ = CameraController.Shake(0.8f);
    Destroy(gameObject, objectDestroyTime);
  }
}
