using System.Collections.Generic;
using UnityEngine;

public class WaypointContainer : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();

    void Awake()
    {
        waypoints.Clear();
        foreach (Transform t in transform)
        {
            waypoints.Add(t);
        }
    }
}