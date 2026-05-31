using UnityEngine;

/// <summary>
/// Attaches to a GameObject to give it a smooth floating (bobbing) animation
/// with optional rotation. Great for entry scene hero objects.
/// </summary>
public class FloatingEffect : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("How far up and down the object bobs (in world units).")]
    public float floatAmplitude = 0.3f;

    [Tooltip("How fast the object bobs up and down.")]
    public float floatFrequency = 1.0f;

    [Header("Rotation Settings")]
    [Tooltip("Enable slow continuous rotation while floating.")]
    public bool enableRotation = true;

    [Tooltip("Rotation speed in degrees per second on each axis.")]
    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

    [Header("Phase Offset")]
    [Tooltip("Randomise start phase so multiple objects don't bob in sync.")]
    public bool randomisePhaseOffset = true;

    // ── internals ──────────────────────────────────────────────
    private Vector3 _originPosition;
    private float   _phaseOffset;

    void Start()
    {
        _originPosition = transform.position;
        _phaseOffset = randomisePhaseOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        // Vertical bob
        float newY = _originPosition.y
                     + Mathf.Sin((Time.time * floatFrequency * Mathf.PI * 2f) + _phaseOffset)
                     * floatAmplitude;

        transform.position = new Vector3(_originPosition.x, newY, _originPosition.z);

        // Optional rotation
        if (enableRotation)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Call this if the object is moved at runtime so the float origin updates.
    /// </summary>
    public void UpdateOrigin()
    {
        _originPosition = transform.position;
    }
}