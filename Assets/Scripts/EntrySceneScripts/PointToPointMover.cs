using System.Collections;
using UnityEngine;

/// <summary>
/// Moves a GameObject from Point A to Point B at a given speed,
/// then instantly teleports back to A and repeats.
/// </summary>
[AddComponentMenu("Scripts/PointToPointMover")]
public class PointToPointMover : MonoBehaviour
{
    // ── Waypoints ──────────────────────────────────────────────
    [Header("Waypoints")]
    [Tooltip("Starting position in world space.")]
    public Transform pointA;

    [Tooltip("Destination position in world space.")]
    public Transform pointB;

    // ── Timing ─────────────────────────────────────────────────
    [Header("Timing")]
    [Tooltip("How long (seconds) the object waits at Point A before moving.")]
    public float waitInterval = 2.0f;

    // ── Movement ───────────────────────────────────────────────
    [Header("Movement")]
    [Tooltip("Travel speed in world units per second.")]
    public float speed = 5.0f;

    public MoveMode moveMode = MoveMode.EasedLerp;

    public enum MoveMode
    {
        LinearLerp, // Constant-speed interpolation
        EasedLerp   // Smooth ease-in/out (SmoothStep)
    }

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("[PointToPointMover] Assign both Point A and Point B in the Inspector.", this);
            enabled = false;
            return;
        }

        transform.position = pointA.position;
        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            // ── Wait at Point A ──
            yield return new WaitForSeconds(waitInterval);

            // ── Travel A → B ──
            yield return StartCoroutine(TravelToB());

            // ── Instantly teleport back to A ──
            transform.position = pointA.position;
        }
    }

    IEnumerator TravelToB()
    {
        Vector3 origin      = pointA.position;
        Vector3 destination = pointB.position;

        float distance      = Vector3.Distance(origin, destination);
        float travelDuration = distance / Mathf.Max(speed, 0.001f); // avoid divide-by-zero
        float elapsed        = 0f;

        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float t      = Mathf.Clamp01(elapsed / travelDuration);
            float factor = (moveMode == MoveMode.EasedLerp) ? Mathf.SmoothStep(0f, 1f, t) : t;

            transform.position = Vector3.Lerp(origin, destination, factor);
            yield return null;
        }

        transform.position = destination;
    }
}