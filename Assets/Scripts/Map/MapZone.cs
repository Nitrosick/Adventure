using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapZone : MonoBehaviour {
  public string id;
  public string zoneName;
  [TextArea(5, 20)] public string description;
  [TextArea(5, 20)] public string descriptionCleared;
  public bool isEmpty;
  public bool secret;
  public Vector3 playerPosition;

  public List<MapZoneType> events = new();
  public LineRenderer[] pathlines;
  public GameObject[] interactiveObjects;

  private MapZoneBattle battle;
  private MeshRenderer markIcon;
  private Transform markQuestIcon;
  private Way[] ways;
  private Transform marker;
  private SpriteRenderer markerRender;
  private Vector3 originalScale;
  private Vector3 originalPosition;

  private int animId = 0;
  private readonly float linesTransparency = 0.6f;
  private readonly float maxDistance = 10f;

  void Awake() {
    battle = transform.GetComponent<MapZoneBattle>();
    marker = transform.Find("Marker");
    markerRender = marker.GetComponent<SpriteRenderer>();
    Transform markIconObj = transform.Find("Mark");

    if (markIconObj != null) {
      markIcon = markIconObj.Find("Icon").GetComponent<MeshRenderer>();
      markQuestIcon = markIconObj.Find("QuestIcon");
    }

    ways = transform.GetComponentsInChildren<Way>();

    if (marker == null || markerRender == null || ways == null || ways.Length < 1) {
      Debug.LogError("Map zone components initialization error");
      return;
    }

    InitPathLines();
    originalScale = marker.localScale;
    originalPosition = marker.position;
    ResetMarker();
  }

  void Start() {
    Dictionary<string, MapZoneData> state = StateManager.zonesState;

    if (state.Count > 0 && state.ContainsKey(id)) {
      if (state[id].events.Count > 0) {
        events = state[id].events;
      }
      else SetCleared();
    }
  }

  void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject() || secret) return;

    Player player = Player.Instance;
    if (player.Move.IsMoving) return;

    float distance = Vector3.Distance(player.transform.position, transform.position);

    if (distance > maxDistance && !StateManager.zonesState.Keys.Contains(id)) {
      MapUI.Instance.ShowZoneTooFar();
      return;
    } else if (!isEmpty) {
      MapUI.Instance.ShowZoneInfo(this);
    }

    MapZone playerZone = player.Move.CurrentZone;
    string[] wayIds = ways.Select(way => way.id).ToArray();
    if (playerZone == this || !wayIds.Contains(playerZone.id)) return;

    HoverMarker();
  }

  void OnMouseExit() {
    MapUI.Instance.HideZoneInfo();
    UnhoverMarker();
  }

  public void ResetMarker() {
    Color color = markerRender.color;
    color.a = secret ? 0f : linesTransparency;
    markerRender.color = color;
  }

  private void HoverMarker() {
    Vector3 targetScale = originalScale * 1.08f;
    Vector3 targetPosition = originalPosition + Vector3.up * 0.05f;

    _ = AnimateMarker(targetScale, targetPosition);

    Color color = markerRender.color;
    color.a = linesTransparency + 0.3f;
    markerRender.color = color;
  }

  private void UnhoverMarker() {
    _ = AnimateMarker(originalScale, originalPosition);
    ResetMarker();
  }

  private void SetCleared() {
    SwitchIconMaterial(false);
    events.Clear();
    SwitchInteractiveObjects();
  }

  public void SetActive() {
    SwitchIconMaterial(true);
    SwitchInteractiveObjects();
  }

  public void SwitchInteractiveObjects() {
    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(!obj.activeSelf);
      }
    }
  }

  public void SwitchIconMaterial(bool on) {
    if (markIcon == null || events.Contains(MapZoneType.Hub)) return;
    Material stone = GameManager.I.stoneMaterial;
    Material gold = GameManager.I.goldMaterial;
    markIcon.material = on ? gold : stone;
  }

  public void ShowQuestionIcon() {
    if (markQuestIcon != null) markQuestIcon.gameObject.SetActive(true);
    if (markIcon != null) markIcon.gameObject.SetActive(false);
  }

  public void HideQuestionIcon() {
    if (markQuestIcon != null) markQuestIcon.gameObject.SetActive(false);
    if (markIcon != null) markIcon.gameObject.SetActive(true);
  }

  public void Visit() {
    if (secret) {
      TriggerAchievement("ac6");
      _ = Toast.Show("star", "Secret zone found");
      secret = false;
      UnhoverMarker();
    }

    Dictionary<string, MapZoneData> state = StateManager.zonesState;
    if (!state.ContainsKey(id)) {
      ShowPathLines();
      float ambushChance = 0;
      if (battle != null) ambushChance = battle.ambushChance;

      state[id] = new MapZoneData {
        events = events,
        visited = true,
        ambushChance = ambushChance
      };
    } else {
      state[id].visited = true;
    }

    if (events.Count > 0 && events[0] == MapZoneType.Hub) {
      StateManager.startPlayerZoneId = id;
      Player.Instance.Move.startZone = this;
    }

    StateManager.currentPlayerZoneId = id;
    transform.GetComponent<MapZoneEvent>().CheckEvents();
  }

  public bool HasAmbush() {
    if (battle != null) return battle.ambushChance > 0;
    return false;
  }

  public void RemoveEvent(MapZoneType type) {
    events = events.Where(e => e != type).ToList();
    if (events.Count == 0) SetCleared();
    StateManager.zonesState[id].events = events;
  }

  private async Task AnimateMarker(Vector3 targetScale, Vector3 targetPosition) {
    int id = ++animId;
    float duration = 0.15f;
    Vector3 startScale = marker.localScale;
    Vector3 startPos = marker.position;
    float elapsed = 0f;

    while (elapsed < duration) {
      if (id != animId) return;
      elapsed += Time.deltaTime;
      float t = elapsed / duration;
      marker.localScale = Vector3.Lerp(startScale, targetScale, t);
      marker.position = Vector3.Lerp(startPos, targetPosition, t);
      await Task.Yield();
    }
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

    while (elapsed < 1f) {
      elapsed += Time.deltaTime;
      color.a = Mathf.Lerp(startAlpha, linesTransparency, elapsed / 1f);
      mat.color = color;
      await Task.Yield();
    }

    color.a = linesTransparency;
    mat.color = color;
  }

  private void TriggerAchievement(string id, float value = 1f) {
    AchievementManager.UpdateAchievement(id, value, false);
  }
}
