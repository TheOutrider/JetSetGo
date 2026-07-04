using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JetAiEnemy : MonoBehaviour
{
    [Header("References")]
    public Rigidbody jetRb;
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;

    [Header("Movement")]
    public float speedMult = 1f;
    public float throttleValue = 1f;
    public float waypointRange = 15f;

    [Header("Rotation")]
    public float rotationSpeed = 90f;

    [Header("Debug")]
    public int currentWaypoint;
    public float currentAngle;

    void Start()
    {
        if (!jetRb) jetRb = GetComponent<Rigidbody>();

        if (waypointContainer)
            waypoints = waypointContainer.waypoints;

        currentWaypoint = 0;

        if (waypoints != null && waypoints.Count > 0)
            transform.LookAt(waypoints[currentWaypoint]);
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) <= waypointRange)
            AdvanceWaypoint();

        Vector3 toWaypoint = waypoints[currentWaypoint].position - transform.position;
        currentAngle = Vector3.SignedAngle(transform.forward, toWaypoint, Vector3.up);

        Debug.DrawRay(transform.position, toWaypoint, Color.yellow);
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;

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
}