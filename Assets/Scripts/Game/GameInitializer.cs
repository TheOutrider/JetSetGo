using UnityEngine;

/// <summary>
/// Example: place this anywhere in your gameplay scene to read the
/// selections stored in GameManager and initialise the correct mode.
/// </summary>
public class GameInitializer : MonoBehaviour
{

    public GameObject CheckpointManager;
    public GameObject SurvivalManager;
    public GameObject CampaignManager;

    private GameObject GameManagerObject, RaceHudObject;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[GameInitializer] GameManager not found! " +
                           "Make sure it exists in the first loaded scene.");
            return;
        }

        Debug.Log($"[GameInitializer] Starting game — {GameManager.Instance}");

        switch (GameManager.Instance.SelectedGameMode)
        {
            case GameManager.GameMode.TimeLimit:
                StartTimeLimitMode();
                break;

            case GameManager.GameMode.Survival:
                StartSurvivalMode();
                break;

            case GameManager.GameMode.EscortMission:
                StartEscortMissionMode();
                break;

            default:
                Debug.LogWarning("[GameInitializer] No game mode set — did you start from MainMenu?");
                break;
        }

        GameManagerObject = GameObject.Find("GameManager");
        RaceHudObject = GameObject.Find("RaceHUD");

        GameManager gameManager = GameManager.Instance;
        RaceHUD raceHud = RaceHudObject.GetComponent<RaceHUD>();
        raceHud.mainMenuButton.onClick.AddListener(gameManager.ReturnToMainMenu);
    }

    private void StartTimeLimitMode()
    {
        Debug.Log("Initialising Time Limit mode...");
        CheckpointManager.SetActive(true);
        // TODO: activate your timer UI, set time limit rules, etc.
    }

    private void StartSurvivalMode()
    {
        Debug.Log("Initialising Survival mode...");
        SurvivalManager.SetActive(true);
        // TODO: spawn enemies, set survival rules, etc.
    }

    private void StartEscortMissionMode()
    {
        Debug.Log("Initialising Escort Mission mode...");
        CampaignManager.SetActive(true);
        // TODO: spawn escort NPC, set objectives, etc.
    }
}