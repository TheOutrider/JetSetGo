//using System.Diagnostics;
using TMPro;
using UnityEngine;

/// <summary>
/// Attach this to the RaceCanvas GameObject in scene 2.
///
/// Hierarchy expected:
///   RaceCanvas  ? attach this script here
///   ??? BackgroundPanel
///   ??? TitleText       (TMP_Text)
///   ??? DescriptionText (TMP_Text)
///
/// On Awake it finds the UIManager singleton and hands it the three
/// references it needs. UIManager immediately shows the waiting state.
/// </summary>
public class RaceCanvasRegistrar : MonoBehaviour
{
    [Tooltip("Drag in RaceCanvas/TitleText")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("Drag in RaceCanvas/DescriptionText")]
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[RaceCanvasRegistrar] UIManager instance not found. " +
                           "Make sure UIManager exists in scene 1 and persists.");
            return;
        }

        UIManager.Instance.RegisterRaceCanvas(gameObject, titleText, descriptionText);
    }
}