using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitUI : MonoBehaviour {
  private Unit unit;
  private Canvas canvas;
  private GameObject marker;
  private GameObject markerActive;
  private GameObject healthBar;
  private RectTransform healthBarFill;
  private Transform effectsPanel;

  private readonly Dictionary<PopupType, string> PopupColors = new() {
    { PopupType.Negative, "#F61010" },
    { PopupType.Positive, "#81D11F" },
    { PopupType.Crit, "#EFBF0D" },
    { PopupType.Neutral, "#FFFFFF" },
    { PopupType.Inactive, "#A0A0A0" }
  };

  private void Awake() {
    unit = transform.GetComponent<Unit>();
    canvas = transform.Find("UI").GetComponent<Canvas>();
    marker = transform.Find("Marker").gameObject;
    markerActive = transform.Find("MarkerActive").gameObject;
    healthBar = transform.Find("UI/HealthBar").gameObject;
    healthBarFill = transform.Find("UI/HealthBar/Fill").GetComponent<RectTransform>();
    effectsPanel = transform.Find("UI/Effects").GetComponent<Transform>();

    if (
      unit == null || canvas == null || marker == null ||
      markerActive == null || healthBar == null || healthBarFill == null ||
      effectsPanel == null
    ) {
      Debug.LogError("Unit UI components initialization error");
      return;
    }

    canvas.worldCamera = Camera.main;
    ClearMarkers();
  }

  private void OnDestroy() {
    ClearMarkers();
  }

  public void MarkAsAlly() {
    marker.SetActive(true);
  }

  public void MarkAsActive() {
    if (unit.Relation == UnitRelation.Emeny) return;
    markerActive.SetActive(true);
    marker.SetActive(false);
  }

  public void MarkAsInactive() {
    if (unit.Relation == UnitRelation.Emeny) return;
    markerActive.SetActive(false);
    marker.SetActive(true);
  }

  public void ClearMarkers() {
    marker.SetActive(false);
    markerActive.SetActive(false);
  }

  public void ShowHealthBar() {
    healthBar.SetActive(true);
  }

  public void HideHealthBar() {
    healthBar.SetActive(false);
  }

  public void UpdateHealth(float total, float current) {
    float percent = Mathf.Clamp01(current / total);
    healthBarFill.sizeDelta = new Vector2(1f * percent, healthBarFill.sizeDelta.y);
  }

  public void UpdateEffects() {
    foreach (Transform child in effectsPanel) {
      Destroy(child.gameObject);
    }

    List<EffectInstance> unitEffects = unit.Effects.ActiveEffects;
    if (unitEffects.Count < 1) return;

    foreach (EffectInstance effect in unitEffects) {
      Instantiate(effect.effectData.icon, effectsPanel);
    }
  }

  public void ShowPopup(string value, PopupType type = PopupType.Neutral) {
    GameObject popup = Instantiate(BattleUI.Instance.damagePopupPrefab, canvas.transform);
    if (popup == null) return;
    TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
    if (text == null) return;

    bool isNumber = float.TryParse(value, out float number);
    string resultValue = value;
    PopupType resultType = type;

    if (isNumber) {
      if (number == 0f) resultType = PopupType.Inactive;
      resultValue = Math.Floor(number).ToString();
    }

    if (ColorUtility.TryParseHtmlString(PopupColors[resultType], out Color color)) text.color = color;
    if (resultType == PopupType.Crit) text.fontSize *= 1.5f;
    text.text = resultValue;

    Destroy(popup, 2);
  }
}
