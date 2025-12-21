using UnityEngine;

public class BlockedPath : MonoBehaviour {
  public string id;
  public Way[] ways;

  public void Init() {
    SetOpacity(0);
  }

  public void Unlock() {
    foreach (Way way in ways) way.blocked = false;
    SetOpacity(1);
    StateManager.unlockedPassages.Add(id);
  }

  private void SetOpacity(int value) {
    LineRenderer renderer = transform.GetComponent<LineRenderer>();
    renderer.material = new Material(renderer.material);
    Color color = renderer.material.color;
    color.a = value;
    renderer.material.color = color;
  }
}
