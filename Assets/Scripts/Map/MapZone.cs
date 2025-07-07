using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapZone : MonoBehaviour
{
  [Header("Common")]
  public int id;
  public Vector3 playerPosition;
  public string zoneName;
  public string description;
  public string descriptionCleared;
  public List<MapZoneType> events;

  [Header("Materials and components")]
  public Material defaultMaterial;
  public Material highlightMaterial;
  public Material stoneMaterial;
  public GameObject[] interactiveObjects;
  private Renderer render;
  private SpriteRenderer markerRender;
  private MeshRenderer markIcon;
  private Way[] ways;
  private Color markerHighlightColor = new(255, 255, 255, 255);

  [Header("Battle")]
  public Unit[] guard;
  public string battlefieldName;
  public int armySlots;
  public BattleReward fixedReward;

  [Header("State")]
  private bool isCleared = false;

  private void Awake() {
    render = GetComponent<Renderer>();
    render.material = defaultMaterial;
    markerRender = transform.Find("Marker").GetComponent<SpriteRenderer>();
    Transform markIconObj = transform.Find("Mark/Icon");
    if (markIconObj != null) markIcon = markIconObj.GetComponent<MeshRenderer>();
    ways = transform.GetComponentsInChildren<Way>();

    if (render == null || markerRender == null || ways == null || ways.Length < 1) {
      Debug.LogError("Map zone components initialization error");
      return;
    }

    if (StateManager.clearedZones.Count > 0) {
      if (!!StateManager.clearedZones[id]) SetCleared();
    }
  }

  private void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject()) return;
    MapUI.ShowZoneInfo(zoneName, isCleared, description, descriptionCleared, guard.Length > 0);

    MapZone playerZone = Player.Instance.GetComponent<PlayerMove>().CurrentZone;
    int[] wayIds = ways.Select(way => way.id).ToArray();
    if (playerZone == this || !wayIds.Contains(playerZone.id)) return;
    render.material = highlightMaterial;
    markerRender.color = markerHighlightColor;
  }

  private void OnMouseExit() {
    MapUI.HideZoneInfo();
    render.material = defaultMaterial;
    markerRender.color = new Color(255, 255, 255, 170);
  }

  public void SetCleared() {
    events.RemoveAt(0);

    if (events.Count < 1) {
      isCleared = true;
      guard = new Unit[] {};
      battlefieldName = "";
      armySlots = 0;
    }

    if (markIcon != null) markIcon.material = stoneMaterial;

    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(false);
      }
    }
  }
}
