using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Remove this line if you're using legacy UI Text instead of TextMeshPro

/// <summary>
/// Attach to your Main Menu canvas/panel.
/// Wire each Button's OnClick() to the corresponding method here,
/// OR assign the Button references in the Inspector and let this script do it.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Game Mode Buttons")]
    [SerializeField] private Button timeLimitButton;
    [SerializeField] private Button survivalButton;
    [SerializeField] private Button escortMissionButton;

    private void Start()
    {
        // Auto-wire buttons if assigned in Inspector.
        if (timeLimitButton != null) timeLimitButton.onClick.AddListener(OnTimeLimitClicked);
        if (survivalButton != null) survivalButton.onClick.AddListener(OnSurvivalClicked);
        if (escortMissionButton != null) escortMissionButton.onClick.AddListener(OnEscortMissionClicked);
    }

    // ── Button Handlers ───────────────────────────────────────────────────────

    public void OnTimeLimitClicked()
    {
        GameManager.Instance.SelectTimeLimit();
    }

    public void OnSurvivalClicked()
    {
        GameManager.Instance.SelectSurvival();
    }

    public void OnEscortMissionClicked()
    {
        GameManager.Instance.SelectEscortMission();
    }
}