using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JetAiEnemy : MonoBehaviour
{
    [Header("References")]
    public Rigidbody jetRb;
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public JetHealth health;

    [Header("Movement")]
    public float speedMult = 1f;
    public float throttleValue = 1f;
    public float waypointRange = 15f;

    [Header("Rotation")]
    public float rotationSpeed = 90f;

    [Header("Combat - Detection")]
    [Tooltip("Tags this AI will consider as valid targets (Player and/or Enemy).")]
    public string[] targetTags = { "Player", "Enemy" };
    public float detectionRange = 60f;
    [Tooltip("Half-angle (degrees) of the forward detection cone. 25 = a 50 degree wide cone.")]
    public float detectionAngle = 25f;
    public LayerMask detectionMask = ~0;
    public float detectionInterval = 0.2f;

    [Header("Combat - Bullets")]
    public Transform bulletFirePoint;
    public GameObject bulletPrefab;
    [Tooltip("Shots per second while a target is in range/cone.")]
    public float bulletFireRate = 4f;

    [Header("Combat - Missiles")]
    public Transform missileFirePoint;
    public GameObject missilePrefab;
    public float missileCooldown = 5f;
    [Tooltip("Only fire missiles at targets farther than this (bullets handle close range).")]
    public float missileMinRange = 15f;

    [Header("Combat - Aiming (Turret)")]
    [Tooltip("How fast the fire points swivel to track a target, independent of body/waypoint rotation.")]
    public float turretTurnSpeed = 220f;

    [Header("Debug")]
    public int currentWaypoint;
    public float currentAngle;
    public Transform currentTarget;

    private float nextDetectionTime;
    private float nextBulletTime;
    private float nextMissileTime;

    // Resting local rotation of the fire points relative to the jet body (their "aligned with flight path" pose)
    private Quaternion bulletRestLocalRot = Quaternion.identity;
    private Quaternion missileRestLocalRot = Quaternion.identity;

    public JetDatabase jetDatabase;

    public GameObject selectedEnemyJetbody;

    void Start()
    {
        if (!jetRb) jetRb = GetComponent<Rigidbody>();
        if (!health) health = GetComponent<JetHealth>();

        if (waypointContainer)
            waypoints = waypointContainer.waypoints;

        currentWaypoint = 0;

        if (waypoints != null && waypoints.Count > 0)
            transform.LookAt(waypoints[currentWaypoint]);

        if (bulletFirePoint) bulletRestLocalRot = bulletFirePoint.localRotation;
        if (missileFirePoint) missileRestLocalRot = missileFirePoint.localRotation;
    }

    void OnEnable()
    {
        if (health) health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (health) health.OnDeath -= HandleDeath;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) <= waypointRange)
            AdvanceWaypoint();

        Vector3 toWaypoint = waypoints[currentWaypoint].position - transform.position;
        currentAngle = Vector3.SignedAngle(transform.forward, toWaypoint, Vector3.up);

        Debug.DrawRay(transform.position, toWaypoint, Color.yellow);

        UpdateDetection();
        AimWeapons();
        HandleWeapons();
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // Waypoint following stays the sole driver of steering/thrust, so combat
        // never pulls the AI off the track - it just aims and fires while racing.
        RotateTowardsWaypoint();

        float currentThrust = throttleValue * speedMult;
        jetRb.AddForce(jetRb.transform.forward * currentThrust, ForceMode.VelocityChange);
    }

    void RotateTowardsWaypoint()
    {
        Vector3 direction = waypoints[currentWaypoint].position - jetRb.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(jetRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        jetRb.MoveRotation(newRotation);
    }

    void AdvanceWaypoint()
    {
        currentWaypoint++;
        if (currentWaypoint >= waypoints.Count) currentWaypoint = 0;
    }

    // ── Combat ───────────────────────────────────────────────────────────────

    void UpdateDetection()
    {
        if (Time.time < nextDetectionTime) return;
        nextDetectionTime = Time.time + detectionInterval;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, detectionMask, QueryTriggerInteraction.Ignore);

        Transform best = null;
        float bestAngle = detectionAngle;

        foreach (Collider hit in hits)
        {
            Transform root = hit.transform.root;
            if (root == transform.root) continue; // skip self

            if (!HasAnyTag(root, targetTags)) continue;

            Vector3 toTarget = root.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toTarget);
            if (angle > bestAngle) continue;

            bestAngle = angle;
            best = root;
        }

        currentTarget = best;
    }

    bool HasAnyTag(Transform t, string[] tags)
    {
        for (int i = 0; i < tags.Length; i++)
            if (t.CompareTag(tags[i])) return true;
        return false;
    }

    /// <summary>
    /// Swivels the fire points toward currentTarget independently of the jet body,
    /// which keeps rotating toward the waypoint. This is what actually lets bullets/missiles
    /// travel toward the enemy instead of always firing straight down the flight path.
    /// Falls back to the fire point's resting pose (aligned with the flight path) when no target.
    /// </summary>
    void AimWeapons()
    {
        if (currentTarget)
        {
            Vector3 toTarget = currentTarget.position - transform.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Quaternion desiredRot = Quaternion.LookRotation(toTarget.normalized, transform.up);

            if (bulletFirePoint)
                bulletFirePoint.rotation = Quaternion.RotateTowards(bulletFirePoint.rotation, desiredRot, turretTurnSpeed * Time.deltaTime);

            if (missileFirePoint)
                missileFirePoint.rotation = Quaternion.RotateTowards(missileFirePoint.rotation, desiredRot, turretTurnSpeed * Time.deltaTime);
        }
        else
        {
            if (bulletFirePoint)
                bulletFirePoint.localRotation = Quaternion.RotateTowards(bulletFirePoint.localRotation, bulletRestLocalRot, turretTurnSpeed * Time.deltaTime);

            if (missileFirePoint)
                missileFirePoint.localRotation = Quaternion.RotateTowards(missileFirePoint.localRotation, missileRestLocalRot, turretTurnSpeed * Time.deltaTime);
        }
    }

    void HandleWeapons()
    {
        if (!currentTarget) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // Bullets: quick, short-to-mid range, fires whenever target is in the detection cone.
        if (distance <= detectionRange && Time.time >= nextBulletTime)
        {
            FireBullet();
            nextBulletTime = Time.time + 1f / Mathf.Max(bulletFireRate, 0.01f);
        }

        // Missiles: slower cooldown, reserved for farther targets.
        if (distance >= missileMinRange && distance <= detectionRange && Time.time >= nextMissileTime)
        {
            FireMissile();
            nextMissileTime = Time.time + missileCooldown;
        }
    }

    void FireBullet()
    {
        if (!bulletPrefab || !bulletFirePoint) return;

        GameObject bulletObj = Instantiate(bulletPrefab, bulletFirePoint.position, bulletFirePoint.rotation);
        AiBullet bullet = bulletObj.GetComponent<AiBullet>();
        if (bullet) bullet.owner = transform;
    }

    void FireMissile()
    {
        if (!missilePrefab || !missileFirePoint) return;

        GameObject missileObj = Instantiate(missilePrefab, missileFirePoint.position, missileFirePoint.rotation);
        AiMissile missile = missileObj.GetComponent<AiMissile>();
        if (missile)
        {
            missile.owner = transform;
            missile.target = currentTarget;
        }
    }

    void HandleDeath()
    {
        // Stop AI logic and let physics/explosion take over.
        enabled = false;

        if (jetRb)
        {
            jetRb.linearVelocity = Vector3.zero;
            jetRb.angularVelocity = Vector3.zero;
            jetRb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 forward = transform.forward * detectionRange;
        Quaternion leftRot = Quaternion.AngleAxis(-detectionAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(detectionAngle, Vector3.up);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftRot * forward);
        Gizmos.DrawRay(transform.position, rightRot * forward);
    }

     public void OnJetSpawned(GameObject body)
    {
        selectedEnemyJetbody = body;
    }

    public void ApplyJetData(JetData data)
    {
        // rollTorque = data.rollTorque;
        // rollStabilize = data.rollStabilize;
        // speedMult = data.speedMultiplier;
        // speedMultAngle = data.speedMultiplierAngle;
    }
}