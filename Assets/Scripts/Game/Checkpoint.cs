using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Set this to the checkpoint's order (0-based). Or let the manager auto-assign.")]
    public int checkpointIndex = 0;

    [Header("Visuals")]
    public MeshRenderer indicatorMesh;
    public Image primaryCheckpointImage, secondaryCheckpointImage, finalCheckpointImage;
    [SerializeField] private TextMeshProUGUI indexText;

    [Header("Role Colors")]
    public Color primaryColor   = Color.green;
    public Color secondaryColor = Color.yellow;
    public Color finalColor     = Color.blue;

    // Only meaningful in single-player (CheckpointManager) time-trial mode
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
        if (indexText) indexText.SetText((checkpointIndex + 1).ToString());
    }

    void OnTriggerEnter(Collider other)
    {
        // Race mode: any racer (player or AI) can trigger it, tracked per-racer via RacerInfo
        if (RaceModeManager.Instance != null)
        {
            RacerInfo racer = other.GetComponentInParent<RacerInfo>();
            if (racer == null) return;

            PlaySound();
            RaceModeManager.Instance.CheckpointReached(racer, checkpointIndex);
            return;
        }

        // Single-player time trial mode (original behaviour)
        if (passed) return;
        if (!other.CompareTag("Player")) return;

        PlaySound();
        CheckpointManager.Instance?.CheckpointReached(checkpointIndex);
    }

    public void SetPassed(bool value)
    {
        passed = value;
        UpdateVisual();
    }

    public bool IsPassed() => passed;

    public void UpdateVisual()
    {
        int nextIndex;
        int totalCount;

        if (RaceModeManager.Instance != null)
        {
            nextIndex  = RaceModeManager.Instance.GetPlayerNextCheckpointIndex();
            totalCount = RaceModeManager.Instance.checkpoints.Count;
        }
        else
        {
            nextIndex  = CheckpointManager.Instance != null ? CheckpointManager.Instance.GetNextCheckpointIndex() : 0;
            totalCount = CheckpointManager.Instance != null ? CheckpointManager.Instance.checkpoints.Count : 0;
        }

        int finalIndex = totalCount - 1;

        bool isPrimary   = checkpointIndex == nextIndex && checkpointIndex != finalIndex;
        bool isSecondary = checkpointIndex == nextIndex + 1 && checkpointIndex != finalIndex;
        bool isFinal     = checkpointIndex == finalIndex;

        if (RaceModeManager.Instance == null)
        {
            // preserve original "passed hides everything" behaviour for time trial
            isPrimary   &= !passed;
            isSecondary &= !passed;
            isFinal     &= !passed;
        }

        if (primaryCheckpointImage)   primaryCheckpointImage.gameObject.SetActive(isPrimary);
        if (secondaryCheckpointImage) secondaryCheckpointImage.gameObject.SetActive(isSecondary);
        if (finalCheckpointImage)     finalCheckpointImage.gameObject.SetActive(isFinal);

        if (!indicatorMesh) return;

        if      (isPrimary)   indicatorMesh.material.color = primaryColor;
        else if (isSecondary) indicatorMesh.material.color = secondaryColor;
        else if (isFinal)     indicatorMesh.material.color = finalColor;
        else                  indicatorMesh.material.color = Color.gray;
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