using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  [TextArea] public string message;
  private readonly float hoverTime = 1f;
  private bool isPointerOver = false;
  private float timer = 0f;

  private void Update() {
    if (!isPointerOver) return;
    timer += Time.deltaTime;
    if (timer >= hoverTime) TooltipManager.Instance.ShowTooltip(message);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (SceneController.Locked) return;
    isPointerOver = true;
    timer = 0f;
  }

  private void OnMouseEnter() {
    if (SceneController.Locked || EventSystem.current.IsPointerOverGameObject()) return;
    isPointerOver = true;
    timer = 0f;
  }

  public void OnPointerExit(PointerEventData eventData) {
    isPointerOver = false;
    timer = 0f;
    TooltipManager.Instance.HideTooltip();
  }

  private void OnMouseExit() {
    isPointerOver = false;
    timer = 0f;
    TooltipManager.Instance.HideTooltip();
  }
}
