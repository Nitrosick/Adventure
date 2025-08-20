using UnityEngine;

public class TileFiller : MonoBehaviour {
  public Vector3 InitPosition { get; private set; }
  public int height;

  private void Awake() {
    InitPosition = transform.position;
  }
}
