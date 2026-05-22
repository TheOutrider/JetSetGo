using UnityEngine;

/// <summary>
/// Example: place this anywhere in your gameplay scene to read the
/// selections stored in GameManager and initialise the correct mode.
/// </summary>
public class GameInitializer : MonoBehaviour
{
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
    }

    private void StartTimeLimitMode()
    {
        Debug.Log("Initialising Time Limit mode...");
        // TODO: activate your timer UI, set time limit rules, etc.
    }

    private void StartSurvivalMode()
    {
        Debug.Log("Initialising Survival mode...");
        // TODO: spawn enemies, set survival rules, etc.
    }

    private void StartEscortMissionMode()
    {
        Debug.Log("Initialising Escort Mission mode...");
        // TODO: spawn escort NPC, set objectives, etc.
    }
}