using UnityEngine;

public class RacerInfo : MonoBehaviour
{
    public string racerName;
    public bool isPlayer;

    [HideInInspector] public int currentCheckpointIndex = 0;
    [HideInInspector] public int totalCheckpoints;
    [HideInInspector] public bool hasFinished;
    [HideInInspector] public float finishTime;

    void Start()
    {
        
    }
}