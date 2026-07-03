using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("3rd Person Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3f, -6f);
    [SerializeField] private float cameraSmoothSpeed = 8f;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Slider moveSlider;

    [Header("Z-Axis Tilt")]
    [SerializeField] private float maxTiltAngle = 45f;      // degrees to tilt when turning
    [SerializeField] private float tiltSmoothSpeed = 5f;    // how fast it tilts and recovers

    // ── cached state ──────────────────────────────────────────────────────────
    private float _moveInput;
    private float _rotateXInput, _rotateYInput;
    private float _currentTiltZ = 0f;

    // ── lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
    }

    // ── Unity callbacks ───────────────────────────────────────────────────────

    private void Update()
    {
        GatherInput();
        ApplyRotation();
        ApplyMovement();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    // ── private helpers ───────────────────────────────────────────────────────

    private void GatherInput()
    {
        _moveInput = moveSlider != null ? moveSlider.value : 0f;

        Vector2 input = lookAction.action.ReadValue<Vector2>();
        _rotateYInput = input.y;
        _rotateXInput = input.x;
    }

    private void ApplyRotation()
    {
        // Yaw and pitch as before
        float rotationXAmount = _rotateXInput * rotationSpeed * Time.deltaTime;
        float rotationYAmount = _rotateYInput * rotationSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, rotationXAmount, Space.Self);
        transform.Rotate(Vector3.right, rotationYAmount, Space.Self);

        // Z tilt: bank when input.y is active, smoothly return to 0 when released
        // Negated so the object leans into the pitch direction naturally
        float targetTiltZ = -_rotateYInput * maxTiltAngle;
        _currentTiltZ = Mathf.Lerp(_currentTiltZ, targetTiltZ, tiltSmoothSpeed * Time.deltaTime);

        // Apply tilt by overwriting only the Z component of eulerAngles
        Vector3 euler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(euler.x, euler.y, _currentTiltZ);
    }

    private void ApplyMovement()
    {
        if (Mathf.Approximately(_moveInput, 0f)) return;

        Vector3 move = transform.forward * (_moveInput * moveSpeed * Time.deltaTime);
        transform.position += move;
    }

    private void UpdateCamera()
    {
        if (cameraTransform == null) return;

        Vector3 desiredPosition = transform.TransformPoint(cameraOffset);

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            desiredPosition,
            cameraSmoothSpeed * Time.deltaTime
        );

        cameraTransform.rotation = Quaternion.Lerp(
            cameraTransform.rotation,
            transform.rotation,
            cameraSmoothSpeed * Time.deltaTime
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 camPos = transform.TransformPoint(cameraOffset);
        Gizmos.DrawSphere(camPos, 0.2f);
        Gizmos.DrawLine(transform.position, camPos);
    }
#endif
}