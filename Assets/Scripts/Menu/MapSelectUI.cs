using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Remove if using legacy UI Text

/// <summary>
/// Attach to your Map Select canvas/panel.
/// Shows which game mode was selected, then lets the player pick a map.
/// </summary>
public class MapSelectUI : MonoBehaviour
{
    [Header("Info Display")]
    [SerializeField] private TMP_Text selectedModeLabel;  // (optional) shows chosen mode

    [Header("Map Buttons")]
    [SerializeField] private Button desertButton;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button urbanButton;

    [Header("Navigation")]
    [SerializeField] private Button backButton;  // goes back to main menu

    private void Start()
    {
        // Show current selection so the player can confirm before picking a map.
        if (selectedModeLabel != null && GameManager.Instance != null)
            selectedModeLabel.text = $"Mode: {GameManager.Instance.SelectedGameMode}";

        // Wire map buttons.
        if (desertButton != null) desertButton.onClick.AddListener(OnDesertClicked);
        if (forestButton != null) forestButton.onClick.AddListener(OnForestClicked);
        if (urbanButton != null) urbanButton.onClick.AddListener(OnUrbanClicked);

        // Wire back button.
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    // ?? Button Handlers ???????????????????????????????????????????????????????

    public void OnDesertClicked() => GameManager.Instance.SelectDesert();
    public void OnForestClicked() => GameManager.Instance.SelectForest();
    public void OnUrbanClicked() => GameManager.Instance.SelectUrban();

    public void OnBackClicked() => GameManager.Instance.ReturnToMainMenu();
}