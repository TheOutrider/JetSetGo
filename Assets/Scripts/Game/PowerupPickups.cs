using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerupPickup : MonoBehaviour
{
    public PowerupType powerupType;
    public int amount = 1;

    [HideInInspector] public WaypointPowerupSpawner spawnerRef;
    [HideInInspector] public int waypointIndex;

    private bool collected;

    void Reset()
    {
        // Make sure the coin's collider works as a trigger by default.
        Collider col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        IPowerupReceiver receiver = other.GetComponentInParent<IPowerupReceiver>();
        if (receiver == null) return;

        collected = true;
        receiver.ReceivePowerup(powerupType, amount);

        if (spawnerRef) spawnerRef.ClearSlot(waypointIndex);

        Destroy(gameObject);
    }
}