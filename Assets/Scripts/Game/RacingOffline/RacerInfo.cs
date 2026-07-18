using UnityEngine;

public class RacerInfo : MonoBehaviour
{
    public string racerName = "Racer";
    public bool isPlayer = false;

    [HideInInspector] public int nextCheckpointIndex = 0;
    [HideInInspector] public bool finished = false;
    [HideInInspector] public float finishTime = -1f;
    [HideInInspector] public int placement = -1;

    public Rigidbody rb;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
    }
}