using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Highlightable : MonoBehaviour {
  private Color highlightColor = Color.white;
  private readonly float highlightIntensity = 0.05f;
  private readonly float fadeDuration = 0.05f;
  private readonly List<Material> materials = new();
  private readonly Dictionary<Material, Color> originalEmission = new();
  private CancellationTokenSource cts;
  private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

  private void Awake() {
    Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

    foreach (Renderer renderer in renderers) {
      foreach (Material material in renderer.materials) {
        if (material == null) continue;

        materials.Add(material);
        material.EnableKeyword("_EMISSION");

        Color emission = material.HasProperty(EmissionColorId)
          ? material.GetColor(EmissionColorId)
          : Color.black;

        originalEmission[material] = emission;
      }
    }
  }

  private void OnMouseEnter() {
    AnimateTo(1f);
  }

  private void OnMouseExit() {
    AnimateTo(0f);
  }

  private void AnimateTo(float target) {
    cts?.Cancel();
    cts = new CancellationTokenSource();
    _ = AnimateEmission(target, cts.Token);
  }

  private async Task AnimateEmission(
    float target,
    CancellationToken token
  ) {
    float start = GetCurrentValue();
    float elapsed = 0f;

    while (elapsed < fadeDuration) {
      if (token.IsCancellationRequested) return;

      elapsed += Time.deltaTime;

      float value = Mathf.Lerp(
        start,
        target,
        elapsed / fadeDuration
      );

      ApplyEmission(value);
      await Task.Yield();
    }

    ApplyEmission(target);
  }

  private void ApplyEmission(float value) {
    foreach (Material material in materials) {
      Color baseEmission = originalEmission[material];

      material.SetColor(
        EmissionColorId,
        baseEmission + highlightColor * (highlightIntensity * value)
      );
    }
  }

  private float GetCurrentValue() {
    if (materials.Count == 0) return 0f;

    Material material = materials[0];

    Color current = material.GetColor(EmissionColorId);
    Color original = originalEmission[material];

    return Mathf.Max(
      current.r - original.r,
      current.g - original.g,
      current.b - original.b
    );
  }

  private void OnDisable() {
    ApplyEmission(0f);

    cts?.Cancel();
    cts?.Dispose();
    cts = null;
  }

  private void OnDestroy() {
    cts?.Cancel();
    cts?.Dispose();
  }
}