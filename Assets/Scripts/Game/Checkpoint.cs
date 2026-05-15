using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Set this to the checkpoint's order (0-based). Or let CheckpointManager auto-assign.")]
    public int checkpointIndex = 0;

    [Header("Visuals")]
    public MeshRenderer indicatorMesh;   // optional: a gate/arch mesh to recolor
    public Color pendingColor = Color.yellow;
    public Color passedColor = Color.green;
    public Color nextColor = Color.cyan;    // highlights the upcoming checkpoint

    [SerializeField] private TextMeshProUGUI indexText;

    private bool passed = false;

    void Start()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;
        UpdateVisual();
        indexText.SetText(checkpointIndex.ToString());
    }

    void OnTriggerEnter(Collider other)
    {
        if (passed) return;
        if (!other.CompareTag("Player")) return;

        CheckpointManager.Instance?.CheckpointReached(checkpointIndex);
    }

    public void SetPassed(bool value)
    {
        passed = value;
        UpdateVisual();
    }

    public bool IsPassed() => passed;

    void UpdateVisual()
    {
        if (!indicatorMesh) return;

        bool isNext = CheckpointManager.Instance != null &&
                      CheckpointManager.Instance.GetNextCheckpointIndex() == checkpointIndex;

        Color target = passed ? passedColor : (isNext ? nextColor : pendingColor);
        indicatorMesh.material.color = target;
    }

    // Draw a visible gizmo in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = passed ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"CP {checkpointIndex}");
    }
}