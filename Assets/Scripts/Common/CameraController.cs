using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
  private static CameraController instance;
  private static CinemachineVirtualCamera cinemachine;
  private static CinemachineBasicMultiChannelPerlin perlin;
  private static Task currentShakeTask;

  private static readonly float moveSpeed = 8f;
  private static readonly float edgeScrollSpeed = 9f;
  private static readonly float edgeSize = 5f;
  private static readonly float focusDuration = 0.7f;
  private static bool isFocusing = false;
  private static bool isShaking = false;
  private static float focusDistance;
  // private static readonly float cameraMargin = 2f;

  [SerializeField]
  private InputActionReference moveInput;
  // public BoxCollider bounds;

  private void Awake() {
    instance = this;
    cinemachine = transform.GetComponent<CinemachineVirtualCamera>();
    perlin = cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    if (cinemachine == null || perlin == null) {
      Debug.LogError("Camera components initialization error");
      return;
    }

    CalculateFocusDistance();
  }

  private void Update() {
    if (isFocusing || SceneController.Locked) return;
    Move();
  }

  private static void Move() {
    Vector3 forward = instance.transform.forward;
    Vector3 right = instance.transform.right;

    forward.y = 0;
    right.y = 0;
    forward.Normalize();
    right.Normalize();

    Vector2 move = instance.moveInput.action.ReadValue<Vector2>();
    Vector3 moveDirection = (right * move.x + forward * move.y).normalized;
    Vector2 mousePos = Mouse.current.position.ReadValue();

    if (mousePos.x <= edgeSize) moveDirection += -right;
    if (mousePos.x >= Screen.width - edgeSize) moveDirection += right;
    if (mousePos.y <= edgeSize) moveDirection += -forward;
    if (mousePos.y >= Screen.height - edgeSize) moveDirection += forward;

    moveDirection.Normalize();

    float speed = moveSpeed * Time.deltaTime;
    if (mousePos.x <= edgeSize || mousePos.x >= Screen.width - edgeSize ||
        mousePos.y <= edgeSize || mousePos.y >= Screen.height - edgeSize) {
      speed = edgeScrollSpeed * Time.deltaTime;
    }

    Vector3 newPosition = instance.transform.position + moveDirection * speed;

    // float camHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
    // float camHalfHeight = Camera.main.orthographicSize;

    // newPosition.x = Mathf.Clamp(newPosition.x, bounds.bounds.min.x + camHalfWidth + cameraMargin, bounds.bounds.max.x - camHalfWidth - cameraMargin);
    // newPosition.z = Mathf.Clamp(newPosition.z, bounds.bounds.min.z + camHalfHeight + cameraMargin, bounds.bounds.max.z - camHalfHeight - cameraMargin);

    instance.transform.position = newPosition;
  }

  private static void CalculateFocusDistance() {
    float height = instance.transform.position.y;
    float angleDegrees = instance.transform.eulerAngles.x;
    float angleRadians = Mathf.Deg2Rad * angleDegrees;
    float result = height / Mathf.Sin(angleRadians);
    focusDistance = result;
  }

  public static async Task FocusOn(Vector3 target, bool immediate = false) {
    isFocusing = true;

    Vector3 direction = instance.transform.forward;
    Vector3 targetPosition = target - direction * focusDistance;
    Vector3 startPos = instance.transform.position;

    if (immediate) {
      instance.transform.position = targetPosition;
      isFocusing = false;
      return;
    }

    float elapsed = 0f;

    while (elapsed < focusDuration) {
      elapsed += Time.deltaTime;
      float t = elapsed / focusDuration;
      t = Mathf.SmoothStep(0, 1, t);

      instance.transform.position = Vector3.Lerp(startPos, targetPosition, t);
      await Task.Yield();
    }

    instance.transform.position = targetPosition;
    isFocusing = false;
  }

  public static async void Shake(float intensity, float customDuration = 0f) {
    if (isShaking && currentShakeTask != null) return;

    float duration = customDuration == 0f ? intensity / 3 : customDuration;
    isShaking = true;
    perlin.m_AmplitudeGain = intensity;
    perlin.m_FrequencyGain = intensity;

    currentShakeTask = Task.Delay((int)(duration * 1000));
    await currentShakeTask;

    perlin.m_AmplitudeGain = 0f;
    perlin.m_FrequencyGain = 0f;
    isShaking = false;
  }
}
