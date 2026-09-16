using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PowerupPrefabEntry
{
    public PowerupType type;
    public GameObject prefab;
}

public class WaypointPowerupSpawner : MonoBehaviour
{
    [Header("References")]
    public WaypointContainer waypointContainer;
    public List<PowerupPrefabEntry> powerupPrefabs;

    [Header("Spawn Rules")]
    [Range(0f, 1f)]
    [Tooltip("Chance that ANY given waypoint gets a powerup spawned near it.")]
    public float spawnChance = 0.5f;

    [Tooltip("Random offset from the waypoint position so the powerup floats nearby, not exactly on it.")]
    public Vector3 spawnOffsetRange = new Vector3(5f, 3f, 5f);

    // waypointIndex -> currently active pickup at that waypoint (null/absent if none)
    private Dictionary<int, PowerupPickup> activePowerups = new Dictionary<int, PowerupPickup>();

    void Start()
    {
        if (!waypointContainer || waypointContainer.waypoints == null) return;

        for (int i = 0; i < waypointContainer.waypoints.Count; i++)
            TrySpawnAt(i);
    }

    void TrySpawnAt(int waypointIndex)
    {
        if (powerupPrefabs == null || powerupPrefabs.Count == 0) return;
        if (Random.value > spawnChance) return;

        PowerupPrefabEntry entry = powerupPrefabs[Random.Range(0, powerupPrefabs.Count)];
        if (!entry.prefab) return;

        Transform wp = waypointContainer.waypoints[waypointIndex];
        Vector3 offset = new Vector3(
            Random.Range(-spawnOffsetRange.x, spawnOffsetRange.x),
            Random.Range(-spawnOffsetRange.y, spawnOffsetRange.y),
            Random.Range(-spawnOffsetRange.z, spawnOffsetRange.z));

        GameObject obj = Instantiate(entry.prefab, wp.position + offset, Quaternion.identity);
        PowerupPickup pickup = obj.GetComponent<PowerupPickup>();
        if (!pickup) return;

        pickup.powerupType = entry.type;
        pickup.spawnerRef = this;
        pickup.waypointIndex = waypointIndex;

        activePowerups[waypointIndex] = pickup;
    }

    /// <summary>Returns the powerup currently sitting near this waypoint, or null.</summary>
    public PowerupPickup GetPowerupAt(int waypointIndex)
    {
        activePowerups.TryGetValue(waypointIndex, out PowerupPickup p);
        return p; // Unity's == null check on a destroyed object still works fine here
    }

    public void ClearSlot(int waypointIndex)
    {
        activePowerups.Remove(waypointIndex);
    }
}