using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI Image (arrow icon) on your HUD canvas.
/// It will rotate to point at the next checkpoint in world space.
/// </summary>
public class RaceHUD : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public RectTransform arrowUI;   // assign a UI arrow Image
    public Button mainMenuButton;

    void Update()
    {
        if (!CheckpointManager.Instance || !playerTransform || !arrowUI) return;
        if (CheckpointManager.Instance.IsRaceFinished()) { arrowUI.gameObject.SetActive(false); return; }

        var mgr = CheckpointManager.Instance;
        int nextIdx = mgr.GetNextCheckpointIndex();

        if (nextIdx >= mgr.checkpoints.Count) { arrowUI.gameObject.SetActive(false); return; }

        arrowUI.gameObject.SetActive(true);
        Vector3 targetPos = mgr.checkpoints[nextIdx].transform.position;
        Vector3 dir = targetPos - playerTransform.position;
        dir.y = 0; // ignore vertical

        // Rotate arrow on the 2D canvas to face the world direction
        float angle = Vector3.SignedAngle(playerTransform.forward, dir, Vector3.up);
        arrowUI.localEulerAngles = new Vector3(0, 0, -angle);
    }
}