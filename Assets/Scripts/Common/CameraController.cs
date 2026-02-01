using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
  private static CameraController Instance;
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

  [SerializeField] private InputActionReference moveInput;
  [SerializeField] private float[] xLimits = { -100, 100 };
  [SerializeField] private float[] zLimits = { -100, 100 };

  void Awake() {
    Instance = this;
    cinemachine = transform.GetComponent<CinemachineVirtualCamera>();
    perlin = cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    if (cinemachine == null || perlin == null) {
      Debug.LogError("Camera components initialization error");
      return;
    }

    CalculateFocusDistance();
  }

  void Start() {
    float angleX = transform.eulerAngles.x;
    float angleY = transform.eulerAngles.y;
    float height = transform.position.y;
    float offsetZ = height / Mathf.Tan(angleX * Mathf.Deg2Rad);
    float offsetX = offsetZ * Mathf.Tan(angleY * Mathf.Deg2Rad);

    Instance.xLimits[0] += offsetX / 2;
    Instance.xLimits[1] += offsetX / 2;
    Instance.zLimits[0] += offsetZ / 2;
    Instance.zLimits[1] += offsetZ / 2;
  }

  void Update() {
    if (isFocusing || SceneController.Locked || StateManager.openedWindows.Count > 0) return;
    if (ShouldMove()) Move();
  }

  private bool ShouldMove() {
    Vector2 move = Instance.moveInput.action.ReadValue<Vector2>();
    Vector2 mousePos = Mouse.current.position.ReadValue();

    bool keyboardInput = move.sqrMagnitude > 0.01f;

    bool edgeInput =
      mousePos.x <= edgeSize ||
      mousePos.x >= Screen.width - edgeSize ||
      mousePos.y <= edgeSize ||
      mousePos.y >= Screen.height - edgeSize;

    return keyboardInput || edgeInput;
  }

  private static void Move() {
    Vector3 forward = Instance.transform.forward;
    Vector3 right = Instance.transform.right;

    forward.y = 0;
    right.y = 0;
    forward.Normalize();
    right.Normalize();

    Vector2 move = Instance.moveInput.action.ReadValue<Vector2>();
    Vector3 moveDirection = (right * move.x + forward * move.y).normalized;

    Vector2 mousePos = Mouse.current.position.ReadValue();
    if (mousePos.x <= edgeSize) moveDirection += -right;
    if (mousePos.x >= Screen.width - edgeSize) moveDirection += right;
    if (mousePos.y <= edgeSize) moveDirection += -forward;
    if (mousePos.y >= Screen.height - edgeSize) moveDirection += forward;

    if (moveDirection == Vector3.zero) return;
    moveDirection.Normalize();

    float speed = moveSpeed * Time.deltaTime;
    if (mousePos.x <= edgeSize || mousePos.x >= Screen.width - edgeSize ||
        mousePos.y <= edgeSize || mousePos.y >= Screen.height - edgeSize) {
      speed = edgeScrollSpeed * Time.deltaTime;
    }

    Vector3 position = Instance.transform.position;
    Vector3 desiredMove = moveDirection * speed;
    Vector3 targetPosition = position + desiredMove;

    if ((targetPosition.x < Instance.xLimits[0] && desiredMove.x < 0) ||
        (targetPosition.x > Instance.xLimits[1] && desiredMove.x > 0)) {
      desiredMove.x = 0;
    }

    if ((targetPosition.z < Instance.zLimits[0] && desiredMove.z < 0) ||
        (targetPosition.z > Instance.zLimits[1] && desiredMove.z > 0)) {
      desiredMove.z = 0;
    }

    Instance.transform.position += desiredMove;
  }

  private static void CalculateFocusDistance() {
    float height = Instance.transform.position.y;
    float angleDegrees = Instance.transform.eulerAngles.x;
    float angleRadians = Mathf.Deg2Rad * angleDegrees;
    float result = height / Mathf.Sin(angleRadians);
    focusDistance = result;
  }

  public static async Task FocusOn(Vector3 target, bool immediate = false) {
    isFocusing = true;

    Vector3 direction = Instance.transform.forward;
    Vector3 targetPosition = target - direction * focusDistance;
    Vector3 startPos = Instance.transform.position;

    if (immediate) {
      Instance.transform.position = targetPosition;
      isFocusing = false;
      return;
    }

    float elapsed = 0f;

    while (elapsed < focusDuration) {
      elapsed += Time.deltaTime;
      float t = elapsed / focusDuration;
      t = Mathf.SmoothStep(0, 1, t);

      Instance.transform.position = Vector3.Lerp(startPos, targetPosition, t);
      await Task.Yield();
    }

    Instance.transform.position = targetPosition;
    isFocusing = false;
  }

  public static async Task Shake(float intensity, float customDuration = 0f) {
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
