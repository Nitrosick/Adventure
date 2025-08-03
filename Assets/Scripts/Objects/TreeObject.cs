using UnityEngine;
using UnityEngine.EventSystems;

public class TreeObject : MonoBehaviour {
  public Tile ParentTile { get; private set; }
  private Animator animator;
  protected readonly int objectDestroyTime = 5;

  private void Awake() {
    ParentTile = transform.GetComponentInParent<Tile>();
    animator = transform.GetComponent<Animator>();

    if (ParentTile == null || animator == null) {
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
    animator.SetTrigger("Fall");
    ParentTile.type = TileType.Open;
    Destroy(gameObject, objectDestroyTime);
  }
}
