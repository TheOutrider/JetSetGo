using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Set this to the checkpoint's order (0-based). Or let CheckpointManager auto-assign.")]
    public int checkpointIndex = 0;

    [Header("Visuals")]
    public MeshRenderer indicatorMesh;   // optional: a gate/arch mesh to recolor

    public Image primaryCheckpointImage, secondaryCheckpointImage, finalCheckpointImage;

    [SerializeField] private TextMeshProUGUI indexText;

    [Header("Role Colors")]
    public Color primaryColor   = Color.green;
    public Color secondaryColor = Color.yellow;
    public Color finalColor     = Color.blue;

    private bool passed = false;

    private AudioSource audioSource;
    public AudioClip passCompleteSound;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        UpdateVisual();
        indexText.SetText((checkpointIndex + 1).ToString());
    }

    void OnTriggerEnter(Collider other)
    {
        if (passed) return;
        if (!other.CompareTag("Player")) return;
        Debug.Log("OBJECT TRIGGERED");  
        PlaySound();

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
        // Determine role of this checkpoint
        // Roles are mutually exclusive; a passed checkpoint shows nothing.
        int nextIndex   = CheckpointManager.Instance != null
                          ? CheckpointManager.Instance.GetNextCheckpointIndex()
                          : 0;
        int totalCount  = CheckpointManager.Instance != null
                          ? CheckpointManager.Instance.checkpoints.Count
                          : 0;
        int finalIndex  = totalCount - 1;

        bool isPrimary   = !passed && checkpointIndex == nextIndex && checkpointIndex != finalIndex;
        bool isSecondary = !passed && checkpointIndex == nextIndex + 1 && checkpointIndex != finalIndex;
        bool isFinal     = !passed && checkpointIndex == finalIndex;

        // ── Images ───────────────────────────────────────────────────────────
        if (primaryCheckpointImage)
            primaryCheckpointImage.gameObject.SetActive(isPrimary);

        if (secondaryCheckpointImage)
            secondaryCheckpointImage.gameObject.SetActive(isSecondary);

        if (finalCheckpointImage)
            finalCheckpointImage.gameObject.SetActive(isFinal);

        // ── Mesh color ────────────────────────────────────────────────────────
        if (!indicatorMesh) return;

        if      (isPrimary)   indicatorMesh.material.color = primaryColor;
        else if (isSecondary) indicatorMesh.material.color = secondaryColor;
        else if (isFinal)     indicatorMesh.material.color = finalColor;
        else                  indicatorMesh.material.color = Color.gray; // passed / hidden
    }

    void OnDrawGizmos()
    {
        Gizmos.color = passed ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }

    public void PlaySound()
    {
        if (passCompleteSound)
            audioSource.PlayOneShot(passCompleteSound, 1);
    }
}