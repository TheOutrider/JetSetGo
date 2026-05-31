using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager that persists across all scenes.
/// Stores selected game mode and map, then loads the appropriate scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ?? Singleton ????????????????????????????????????????????????????????????

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this duplicate.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Caps the frame rate at 60 FPS
        Application.targetFrameRate = 60;
        // Keep this GameObject alive when loading new scenes.
        DontDestroyOnLoad(gameObject);
    }

    // ?? Enums ?????????????????????????????????????????????????????????????????

    public enum GameMode
    {
        None,
        TimeLimit,
        Survival,
        EscortMission
    }

    public enum MapChoice
    {
        None,
        Tropics,
        Cityline,
        NeonJungle    
    }

    public GameMode SelectedGameMode { get; private set; } = GameMode.None;
    public MapChoice SelectedMap { get; private set; } = MapChoice.None;

    // Make sure these EXACTLY match the scene names in File > Build Settings.

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string mapSelectScene = "MapSelect";

    public void OnGameModeSelected(GameMode mode)
    {
        SelectedGameMode = mode;
        SelectedMap = MapChoice.None;   // reset any previous map choice

        Debug.Log($"[GameManager] Game mode selected: {mode}");

        // Go to map selection screen.
        //SceneManager.LoadScene(mapSelectScene);
    }

    // Convenience wrappers so UI buttons can call these directly via UnityEvent.
    public void SelectTimeLimit() => OnGameModeSelected(GameMode.TimeLimit);
    public void SelectSurvival() => OnGameModeSelected(GameMode.Survival);
    public void SelectEscortMission() => OnGameModeSelected(GameMode.EscortMission);

    // ?? Step 2 : Map Selection (called from MapSelectUI) ??????????????????????

    public void OnMapSelected(MapChoice map)
    {
        SelectedMap = map;

        Debug.Log($"[GameManager] Map selected: {map}");

        //LoadGameScene();
    }

    public void SelectTropics() => OnMapSelected(MapChoice.Tropics);
    public void SelectCityLine() => OnMapSelected(MapChoice.Cityline);
    public void SelectNeonJungle() => OnMapSelected(MapChoice.NeonJungle);


    private void LoadGameScene()
    {
        if (SelectedMap == MapChoice.None)
        {
            Debug.LogError("[GameManager] No scene mapped for: " + SelectedMap);
            return;
        }

        Debug.Log($"[GameManager] Loading scene: {SelectedMap} " +
                  $"| Mode: {SelectedGameMode} | Map: {SelectedMap}");

        SceneManager.LoadScene(SelectedMap.ToString());
    }


    /// <summary>Reset state and return to the main menu.</summary>
    public void ReturnToMainMenu()
    {
        SelectedGameMode = GameMode.None;
        SelectedMap = MapChoice.None;
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>Quick read-out of current session data (useful for debugging).</summary>
    public override string ToString() =>
        $"GameManager | Mode: {SelectedGameMode} | Map: {SelectedMap}";
}