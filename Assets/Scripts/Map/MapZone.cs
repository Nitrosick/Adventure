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
  public bool isEmpty = false;

  [Header("Materials and components")]
  public Material defaultMaterial;
  public Material highlightMaterial;
  public Material stoneMaterial;
  public GameObject[] interactiveObjects;
  protected Renderer render;
  protected SpriteRenderer markerRender;
  protected MeshRenderer markIcon;
  protected Way[] ways;
  protected Color markerHighlightColor = new(255, 255, 255, 255);

  protected void Awake() {
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

    Dictionary<int, List<MapZoneType>> state = StateManager.zonesState;
    if (state.Count > 0 && state[id] != null) {
      if (state[id].Count > 0) events = state[id];
      else SetCleared();
    }
  }

  protected void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject()) return;
    MapUI.ShowZoneInfo(zoneName, description, descriptionCleared, events, isEmpty);

    MapZone playerZone = Player.Instance.GetComponent<PlayerMove>().CurrentZone;
    int[] wayIds = ways.Select(way => way.id).ToArray();
    if (playerZone == this || !wayIds.Contains(playerZone.id)) return;
    render.material = highlightMaterial;
    markerRender.color = markerHighlightColor;
  }

  protected void OnMouseExit() {
    MapUI.HideZoneInfo();
    render.material = defaultMaterial;
    markerRender.color = new Color(255, 255, 255, 170);
  }

  public virtual void SetCleared() {
    if (markIcon != null) markIcon.material = stoneMaterial;

    if (interactiveObjects != null && interactiveObjects.Length > 0) {
      foreach (GameObject obj in interactiveObjects) {
        obj.SetActive(!obj.activeSelf);
      }
    }
  }
}
