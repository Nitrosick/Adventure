using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour {
  private static IconDatabase IconDatabase;

  private static Transform panel;
  private static Image image;
  private static TextMeshProUGUI message;

  private static CancellationTokenSource cts;

  private void Awake() {
    IconDatabase = Resources.Load<IconDatabase>("Databases/IconDatabase");

    panel = transform.Find("Toast/Panel").GetComponent<Transform>();
    image = panel.Find("Icon").GetComponent<Image>();
    message = panel.Find("Text").GetComponent<TextMeshProUGUI>();

    if (panel == null || image == null || message == null) {
      Debug.LogError("Info popup components initialization error");
    }
  }

  public static async Task Show(string icon, string text, int delay = 5) {
    cts?.Cancel();
    cts = new CancellationTokenSource();

    image.sprite = IconDatabase.GetIcon(icon);
    message.text = text;
    panel.gameObject.SetActive(true);
    UpdateSize();

    try {
      await Task.Delay(delay * 1000, cts.Token);
      Hide();
    } catch (TaskCanceledException) { }
  }

  private static void Hide() {
    panel.gameObject.SetActive(false);
    image.sprite = null;
    message.text = "";
  }

  private static void UpdateSize() {
    message.ForceMeshUpdate();
    float width = message.preferredWidth;
    Vector2 size = message.rectTransform.sizeDelta;
    size.x = width;
    message.rectTransform.sizeDelta = size;
  }
}
