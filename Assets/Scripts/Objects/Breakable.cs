using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Breakable : MonoBehaviour {
  public Tile ParentTile { get; private set; }
  public GameObject destroyedPrefab;
  protected readonly int objectDestroyTime = 10;

  private void Awake() {
    ParentTile = transform.GetComponentInParent<Tile>();

    if (ParentTile == null || destroyedPrefab == null) {
      Debug.LogError("Breakable object components initialization error");
    }
  }

  void OnMouseEnter() {
    if (EventSystem.current.IsPointerOverGameObject()) return;
    ParentTile.Hover();
  }

  void OnMouseExit() {
    ParentTile.Unhover();
  }

  public void Break() {
    List<GameObject> crashedObjects = new() { };

    foreach (Transform child in transform) {
      GameObject obj = Instantiate(destroyedPrefab, child.transform.position, child.transform.rotation);
      crashedObjects.Add(obj);
      Destroy(child.gameObject);
    }

    foreach (GameObject obj in crashedObjects) {
      Destroy(obj, objectDestroyTime);
    }

    ParentTile.DropLoot();
    Destroy(gameObject);
  }
}
