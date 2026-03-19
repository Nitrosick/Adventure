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
  public LineRenderer[] pathlines;

  public GameObject[] interactiveObjects;
  public List<Quest> QuestsList { get; set; } = new() { };
  private Renderer auraRender;
  private SpriteRenderer markerRender;
  private MeshRenderer markIcon;
  private Transform markQuestIcon;
  private Way[] ways;

  private readonly float fadeDuration = 1f;
  private readonly float linesTransparency = 0.6f;
  private readonly float maxDistance = 10f;

  void Awake() {
    auraRender = transform.GetComponent<Renderer>();
    markerRender = transform.Find("Marker").GetComponent<SpriteRenderer>();

    Transform markIconObj = transform.Find("Mark");
    if (markIconObj != null) {
      markIcon = markIconObj.Find("Icon").GetComponent<MeshRenderer>();
      markQuestIcon = markIconObj.Find("QuestIcon");
    }

    ways = transform.GetComponentsInChildren<Way>();

    if (auraRender == null || markerRender == null || ways == null || ways.Length < 1) {
      Debug.LogError("Map zone components initialization error");
      return;
    }

    InitPathLines();
    InitMarker();
  }

  void Start() {
    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    if (state.Count > 0 && state.ContainsKey(id)) {
      if (state[id].events.Count > 0) events = state[id].events;
      else SetCleared();
    }

    auraRender.material = GameManager.I.transparentMaterial;
  }

  private void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject() || secret) return;

    float distance = Vector3.Distance(Player.Instance.transform.position, transform.position);

    if (distance > maxDistance && !StateManager.zonesState.Keys.Contains(id)) {
      MapUI.Instance.ShowZoneTooFar();
      return;
    } else if (!isEmpty) {
      MapUI.Instance.ShowZoneInfo(this);
    }

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
    auraRender.material = GameManager.I.transparentMaterial;
    InitMarker();
  }

  private void SetCleared() {
    SwitchIcon(false);
    events.Clear();
    SwitchInteractiveObjects();
  }

  public void SetActive() {
    SwitchIcon(true);
    SwitchInteractiveObjects();
  }

  public void SwitchInteractiveObjects() {
    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(!obj.activeSelf);
      }
    }
  }

  public void SwitchIcon(bool on) {
    if (markIcon == null) return;
    Material stone = GameManager.I.stoneMaterial;
    Material gold = GameManager.I.goldMaterial;
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

    if (events.Count > 0 && events[0] == MapZoneType.Hub) {
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
    foreach (LineRenderer path in pathlines) {
      path.material = new Material(path.material);
      Color color = path.material.color;
      color.a = 0;
      path.material.color = color;
    }
  }

  public void ShowPathLines() {
    foreach (LineRenderer path in pathlines) {
      _ = PathLineFade(path);
    }
  }

  private async Task PathLineFade(LineRenderer path) {
    Material mat = path.material;
    Color color = mat.color;

    if (Mathf.Approximately(color.a, linesTransparency)) return;

    float startAlpha = color.a;
    float elapsed = 0f;

    while (elapsed < fadeDuration) {
      elapsed += Time.deltaTime;
      color.a = Mathf.Lerp(startAlpha, linesTransparency, elapsed / fadeDuration);
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
