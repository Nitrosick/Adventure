using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapZone : MonoBehaviour {
  public int id;
  public Vector3 playerPosition;
  public string zoneName;
  [TextArea(5, 20)] public string description;
  [TextArea(5, 20)] public string descriptionCleared;
  public List<MapZoneType> events;
  public bool isEmpty;
  public bool secret;

  public GameObject[] interactiveObjects;
  protected Renderer auraRender;
  protected SpriteRenderer markerRender;
  protected MeshRenderer markIcon;
  protected Way[] ways;
  protected Transform pathLines;

  private readonly float fadeDuration = 1f;
  private readonly float linesTransparency = 0.6f;

  protected void Awake() {
    auraRender = GetComponent<Renderer>();
    markerRender = transform.Find("Marker").GetComponent<SpriteRenderer>();
    Transform markIconObj = transform.Find("Mark/Icon");
    if (markIconObj != null) markIcon = markIconObj.GetComponent<MeshRenderer>();
    ways = transform.GetComponentsInChildren<Way>();
    pathLines = transform.Find("Pathes");

    if (auraRender == null || markerRender == null || ways == null || pathLines == null || ways.Length < 1) {
      Debug.LogError("Map zone components initialization error");
      return;
    }

    InitPathLines();
    InitMarker();
  }

  private void Start() {
    Dictionary<int, List<MapZoneType>> state = StateManager.zonesState;
    if (state.Count > 0 && state[id] != null) {
      if (state[id].Count > 0) events = state[id];
      else SetCleared();
    }

    auraRender.material = MapZoneManager.Instance.defaultMaterial;
  }

  protected void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject() || secret) return;
    MapUI.Instance.ShowZoneInfo(zoneName, description, descriptionCleared, events, isEmpty);

    MapZone playerZone = Player.Instance.GetComponent<PlayerMove>().CurrentZone;
    int[] wayIds = ways.Select(way => way.id).ToArray();
    if (playerZone == this || !wayIds.Contains(playerZone.id)) return;

    auraRender.material = MapZoneManager.Instance.highlightMaterial;
    Color color = markerRender.color;
    color.a = linesTransparency + 0.3f;
    markerRender.color = color;
  }

  protected void OnMouseExit() {
    MapUI.Instance.HideZoneInfo();

    auraRender.material = MapZoneManager.Instance.defaultMaterial;
    InitMarker();
  }

  public virtual void SetCleared() {
    if (markIcon != null) markIcon.material = MapZoneManager.Instance.stoneMaterial;

    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(!obj.activeSelf);
      }
    }
  }

  public void Visit() {
    if (secret) {
      _ = Toast.Show("star", "Secret zone found");
      secret = false;
      InitMarker();
    }

    if (!StateManager.visitedZones.Contains(id)) {
      ShowPathLines();
      StateManager.visitedZones.Add(id);
    }

    StateManager.currentPlayerZoneId = id;
    transform.GetComponent<MapZoneEvent>().CheckEvents();
  }

  public void UnshiftEvent() {
    if (events[0] == MapZoneType.Ambush) return;
    events.RemoveAt(0);

    if (events.Count == 0) {
      SetCleared();
      MapUI.Instance.HideInteractableButton();
    }

    StateManager.zonesState[id] = events;
  }

  public void RemoveAmbush() {
    events = events
      .Where(e => e != MapZoneType.Ambush)
      .ToList();
    StateManager.zonesState[id] = events;
  }

  public void InitMarker() {
    Color color = markerRender.color;
    color.a = secret ? 0f : linesTransparency;
    markerRender.color = color;
  }

  private void InitPathLines() {
    foreach (Transform path in pathLines) {
      LineRenderer renderer = path.GetComponent<LineRenderer>();
      renderer.material = new Material(renderer.material);
      Color color = renderer.material.color;
      color.a = 0;
      renderer.material.color = color;
    }
  }

  public void ShowPathLines() {
    foreach (Transform path in pathLines) {
      _ = PathLineFade(path);
    }
  }

  private async Task PathLineFade(Transform path) {
    LineRenderer renderer = path.GetComponent<LineRenderer>();
    Material mat = renderer.material;
    Color color = mat.color;
    float elapsed = 0f;

    while (elapsed < fadeDuration) {
      elapsed += Time.deltaTime;
      color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
      mat.color = color;
      await Task.Yield();
    }

    color.a = linesTransparency;
    mat.color = color;
  }
}
