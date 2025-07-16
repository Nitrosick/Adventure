using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
  private static IconDatabase IconDatabase;
  public static bool Locked { get; private set; } = false;

  // Background
  private static GameObject background;
  private static Image overlay;
  private static readonly float fadeDuration = 2f;

  // Event panel
  private static GameObject eventPanel;
  private static Image eventIcon;
  private static TextMeshProUGUI eventText;

  private void Awake() {
    IconDatabase = Resources.Load<IconDatabase>("Databases/IconDatabase");

    background = transform.Find("Background/Image").gameObject;
    overlay = transform.Find("Overlay/Image").GetComponent<Image>();
    eventPanel = transform.Find("Event/Panel").gameObject;
    eventIcon = transform.Find("Event/Panel/IconPlace/Icon").GetComponent<Image>();
    eventText = transform.Find("Event/Panel/Text").GetComponent<TextMeshProUGUI>();

    if (background == null || overlay == null || eventPanel == null || eventIcon == null || eventText == null) {
      Debug.LogError("Scene controller components initialization error");
      return;
    }

    Unlock();
  }

  private void Start() { _ = FadeOut(); }
  private void OnDestroy() { HideEventInfo(); }

  public static void Lock() { Locked = true; }
  public static void Unlock() { Locked = false; }

  public static void ShowBackground() { background.SetActive(true); }
  public static void HideBackground() { background.SetActive(false); }

  public static void SwitchScene(string name) {
    Lock();
    overlay.raycastTarget = true;
    _ = FadeIn(() => SceneManager.LoadScene(name));
  }

  public static async Task FadeIn(Action onComplete = null) {
    await Fade(0f, 1f, true);
    onComplete?.Invoke();
  }

  public static async Task FadeOut() {
    await Fade(1f, 0f, false);
  }

  private static async Task Fade(float from, float to, bool easeIn) {
    float time = 0f;
    Color color = overlay.color;

    while (time < fadeDuration) {
      time += Time.deltaTime;

      float t = Mathf.Clamp01(time / fadeDuration);
      float curvedT = easeIn ? Mathf.SmoothStep(0f, 1f, t) : 1f - Mathf.SmoothStep(0f, 1f, 1f - t);
      float alpha = Mathf.Lerp(from, to, curvedT);

      overlay.color = new Color(color.r, color.g, color.b, alpha);
      await Task.Yield();
    }

    overlay.color = new Color(color.r, color.g, color.b, to);
  }

  public static void ShowEventInfo(string icon, string text) {
    eventPanel.SetActive(true);
    Sprite sprite = IconDatabase.GetIcon(icon);
    if (sprite != null) eventIcon.sprite = sprite;
    eventText.text = text;
  }

  public static void HideEventInfo() {
    eventText.text = "";
  }
}
