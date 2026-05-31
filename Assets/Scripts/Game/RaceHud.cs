using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rotates a 3D arrow so it points exactly toward
/// the next checkpoint in full 3D space.
/// </summary>
public class RaceHUD : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    // 3D Arrow object
    public Transform arrow3D;

    public Button mainMenuButton;

    void Update()
    {
        if (!CheckpointManager.Instance || !playerTransform || !arrow3D)
            return;

        if (CheckpointManager.Instance.IsRaceFinished())
        {
            arrow3D.gameObject.SetActive(false);
            return;
        }

        var mgr = CheckpointManager.Instance;
        int nextIdx = mgr.GetNextCheckpointIndex();

        if (nextIdx >= mgr.checkpoints.Count)
        {
            arrow3D.gameObject.SetActive(false);
            return;
        }

        arrow3D.gameObject.SetActive(true);

        Vector3 targetPos = mgr.checkpoints[nextIdx].transform.position;

        // Full 3D direction
        Vector3 dir = targetPos - arrow3D.position;

        // Avoid invalid rotation
        if (dir.sqrMagnitude < 0.001f)
            return;

        // Rotate in ALL axes toward checkpoint
        arrow3D.rotation = Quaternion.LookRotation(dir);
    }
}




// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// Attach to a UI Image (arrow icon) on your HUD canvas.
// /// It will rotate to point at the next checkpoint in world space.
// /// </summary>
// public class RaceHUD : MonoBehaviour
// {
//     [Header("References")]
//     public Transform playerTransform;
//     public RectTransform arrowUI;   // assign a UI arrow Image
//     public Button mainMenuButton;

//     void Update()
//     {
//         if (!CheckpointManager.Instance || !playerTransform || !arrowUI) return;
//         if (CheckpointManager.Instance.IsRaceFinished()) { arrowUI.gameObject.SetActive(false); return; }

//         var mgr = CheckpointManager.Instance;
//         int nextIdx = mgr.GetNextCheckpointIndex();

//         if (nextIdx >= mgr.checkpoints.Count) { arrowUI.gameObject.SetActive(false); return; }

//         arrowUI.gameObject.SetActive(true);
//         Vector3 targetPos = mgr.checkpoints[nextIdx].transform.position;
//         Vector3 dir = targetPos - playerTransform.position;
//         dir.y = 0; // ignore vertical

//         // Rotate arrow on the 2D canvas to face the world direction
//         float angle = Vector3.SignedAngle(playerTransform.forward, dir, Vector3.up);
//         arrowUI.localEulerAngles = new Vector3(0, 0, -angle);
//     }
// }