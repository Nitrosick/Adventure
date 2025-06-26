using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour {
  public static TooltipManager Instance;

  [SerializeField] private GameObject tooltipPanel;
  [SerializeField] private TextMeshProUGUI tooltipText;

  private void Awake() {
    Instance = this;
    HideTooltip();
  }

  public void ShowTooltip(string message) {
    tooltipText.text = message;
    tooltipPanel.SetActive(true);
  }

  public void HideTooltip() {
    tooltipPanel.SetActive(false);
  }
}
