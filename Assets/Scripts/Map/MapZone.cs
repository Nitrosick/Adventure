using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapZone : MonoBehaviour {
  public string id;
  public Vector3 playerPosition;
  public string zoneName;
  [TextArea(5, 20)] public string description;
  [TextArea(5, 20)] public string descriptionCleared;
  public List<MapZoneType> events = new();
  public bool isEmpty;
  public bool secret;

  public GameObject[] interactiveObjects;
  public List<Quest> QuestsList { get; set; } = new() { };
  private Renderer auraRender;
  private SpriteRenderer markerRender;
  private MeshRenderer markIcon;
  private Transform markQuestIcon;
  private Way[] ways;
  private Transform pathLines;

  private readonly float fadeDuration = 1f;
  private readonly float linesTransparency = 0.6f;

  private void Awake() {
    auraRender = transform.GetComponent<Renderer>();
    markerRender = transform.Find("Marker").GetComponent<SpriteRenderer>();

    Transform markIconObj = transform.Find("Mark");
    if (markIconObj != null) {
      markIcon = markIconObj.Find("Icon").GetComponent<MeshRenderer>();
      markQuestIcon = markIconObj.Find("QuestIcon");
    }

    ways = transform.GetComponentsInChildren<Way>();
    pathLines = transform.Find("Pathes");

    if (auraRender == null || markerRender == null || ways == null || ways.Length < 1) {
      Debug.LogError("Map zone components initialization error");
      return;
    }

    InitPathLines();
    InitMarker();
  }

  private void Start() {
    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    if (state.Count > 0 && state.ContainsKey(id)) {
      if (state[id].events.Count > 0) events = state[id].events;
      else SetCleared();
    }

    auraRender.material = MapZoneManager.Instance.defaultMaterial;
  }

  private void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject() || secret) return;
    MapUI.Instance.ShowZoneInfo(zoneName, description, descriptionCleared, events, isEmpty);

    MapZone playerZone = Player.Instance.GetComponent<PlayerMove>().CurrentZone;
    string[] wayIds = ways.Select(way => way.id).ToArray();
    if (playerZone == this || !wayIds.Contains(playerZone.id)) return;

    auraRender.material = MapZoneManager.Instance.highlightMaterial;
    Color color = markerRender.color;
    color.a = linesTransparency + 0.3f;
    markerRender.color = color;
  }

  private void OnMouseExit() {
    MapUI.Instance.HideZoneInfo();
    auraRender.material = MapZoneManager.Instance.defaultMaterial;
    InitMarker();
  }

  private void SetCleared() {
    if (isEmpty) return;
    SwitchIcon(false);
    events.Clear();
    isEmpty = true;
    SwitchInteractiveObjects();
  }

  public void SetActive() {
    SwitchIcon(true);
    isEmpty = false;
    SwitchInteractiveObjects();
  }

  private void SwitchInteractiveObjects() {
    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(!obj.activeSelf);
      }
    }
  }

  public void SwitchIcon(bool on) {
    if (markIcon == null) return;
    Material stone = MapZoneManager.Instance.stoneMaterial;
    Material gold = MapZoneManager.Instance.goldMaterial;
    markIcon.material = on ? gold : stone;
  }

  public void SwitchQuestIcon() {
    if (markIcon == null || markQuestIcon == null) return;

    if (markQuestIcon.gameObject.activeSelf) {
      markQuestIcon.gameObject.SetActive(false);
      markIcon.gameObject.SetActive(true);
    } else {
      markQuestIcon.gameObject.SetActive(true);
      markIcon.gameObject.SetActive(false);
    }
  }

  public void Visit() {
    if (secret) {
      TriggerAchievement("ac6");
      _ = Toast.Show("star", "Secret zone found");
      secret = false;
      InitMarker();
    }

    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    if (!state.ContainsKey(id)) {
      ShowPathLines();
      state[id] = new MapZoneData {
        events = events,
        visited = true
      };
    }
    else {
      state[id].visited = true;
    }

    if (events.Count > 0 && events[0] == MapZoneType.Home) {
      StateManager.startPlayerZoneId = id;
      Player.Instance.Move.startZone = this;
    }

    StateManager.currentPlayerZoneId = id;
    transform.GetComponent<MapZoneEvent>().CheckEvents();
  }

  public void RemoveEvent(MapZoneType type) {
    events = events.Where(e => e != type).ToList();
    if (events.Count == 0) SetCleared();
    StateManager.zonesState[id].events = events;
  }

  public void InitMarker() {
    Color color = markerRender.color;
    color.a = secret ? 0f : linesTransparency;
    markerRender.color = color;
  }

  private void InitPathLines() {
    if (pathLines == null) return;

    foreach (Transform path in pathLines) {
      LineRenderer renderer = path.GetComponent<LineRenderer>();
      renderer.material = new Material(renderer.material);
      Color color = renderer.material.color;
      color.a = 0;
      renderer.material.color = color;
    }
  }

  public void ShowPathLines() {
    if (pathLines == null) return;

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

  public void ActivateQuest(Quest quest) {
    QuestsList.Insert(0, quest);
    events.Insert(0, MapZoneType.Quest);
    SetActive();
  }

  private void TriggerAchievement(string id, float value = 1f) {
    AchievementManager.UpdateAchievement(id, value, false);
  }
}
