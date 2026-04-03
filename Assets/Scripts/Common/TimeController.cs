using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour {
  private readonly float dayLength = 1440f; // 24 hours
  private readonly float dayStart = 240f; // 4:00
  private readonly float nightStart = 1200f; // 20:00
  public float currentTime = 720f; // 12:00
  private float timer = 0f;
  private float visualTime;

  public Light sun;
  public AnimationCurve lightIntensity;
  public Gradient lightColor;
  public TextMeshProUGUI uiTime;
  public Image iconImage;
  public Sprite sunIcon;
  public Sprite moonIcon;

  void Awake() {
    if (sun == null || uiTime == null || iconImage == null || sunIcon == null || moonIcon == null) {
      Debug.LogError("Time controller components initialization error");
      return;
    }

    currentTime = StateManager.dayTime;
    if (currentTime >= dayStart && currentTime < nightStart) iconImage.sprite = sunIcon;
    else iconImage.sprite = moonIcon;
  }

  void Update() {
    timer += Time.deltaTime;

    if (timer >= 1f) {
      timer -= 1f;
      currentTime += 1f;
      if (currentTime >= dayLength) currentTime = 0f;
    }

    visualTime = Mathf.Lerp(visualTime, currentTime, Time.deltaTime * 5f);
    UpdateSun(visualTime);
    UpdateIcon();
    if (uiTime != null) uiTime.text = GetTimeString();
    StateManager.dayTime = currentTime;
  }

  private void UpdateSun(float time) {
    float normalizedTime = time / dayLength;
    float angle = normalizedTime * 360f - 95f;

    sun.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
    sun.intensity = lightIntensity.Evaluate(normalizedTime);
    sun.color = lightColor.Evaluate(normalizedTime);
  }

  private void UpdateIcon() {
    if (currentTime == dayStart) iconImage.sprite = sunIcon;
    else if (currentTime == nightStart) iconImage.sprite = moonIcon;
  }

  private string GetTimeString() {
    int totalMinutes = Mathf.FloorToInt(currentTime);
    int hours = totalMinutes / 60;
    int minutes = totalMinutes % 60;
    return $"{hours:00}:{minutes:00}";
  }
}
