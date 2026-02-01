using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LogItem : MonoBehaviour {
  private TextMeshProUGUI text;
  private readonly int lifetime = 7000;

  void Awake() {
    text = transform.GetComponent<TextMeshProUGUI>();
    if (text == null) Debug.LogError("Log item UI components initialization error");
  }

  public async void Init(string message) {
    text.text = message;
    await Task.Delay(Mathf.RoundToInt(lifetime));
    if (gameObject == null) return;
    Destroy(gameObject);
  }
}
