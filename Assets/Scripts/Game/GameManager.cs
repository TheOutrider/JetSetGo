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
        Desert,
        Forest,
        Urban      // Add / rename maps to match your project
    }

    // ?? Stored Session Data ???????????????????????????????????????????????????

    public GameMode SelectedGameMode { get; private set; } = GameMode.None;
    public MapChoice SelectedMap { get; private set; } = MapChoice.None;

    // ?? Scene Names ???????????????????????????????????????????????????????????
    // Make sure these EXACTLY match the scene names in File > Build Settings.

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string mapSelectScene = "MapSelect";
    [SerializeField] private string desertScene = "Desert";
    [SerializeField] private string forestScene = "Forest";
    [SerializeField] private string urbanScene = "Urban";

    // ?? Step 1 : Game Mode Selection (called from MainMenuUI) ?????????????????

    public void OnGameModeSelected(GameMode mode)
    {
        SelectedGameMode = mode;
        SelectedMap = MapChoice.None;   // reset any previous map choice

        Debug.Log($"[GameManager] Game mode selected: {mode}");

        // Go to map selection screen.
        SceneManager.LoadScene(mapSelectScene);
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

        LoadGameScene();
    }

    // Convenience wrappers for UI buttons.
    public void SelectDesert() => OnMapSelected(MapChoice.Desert);
    public void SelectForest() => OnMapSelected(MapChoice.Forest);
    public void SelectUrban() => OnMapSelected(MapChoice.Urban);

    // ?? Scene Loading ?????????????????????????????????????????????????????????

    private void LoadGameScene()
    {
        string targetScene = SelectedMap switch
        {
            MapChoice.Desert => desertScene,
            MapChoice.Forest => forestScene,
            MapChoice.Urban => urbanScene,
            _ => null
        };

        if (targetScene == null)
        {
            Debug.LogError("[GameManager] No scene mapped for: " + SelectedMap);
            return;
        }

        Debug.Log($"[GameManager] Loading scene: {targetScene} " +
                  $"| Mode: {SelectedGameMode} | Map: {SelectedMap}");

        SceneManager.LoadScene(targetScene);
    }

    // ?? Utility ???????????????????????????????????????????????????????????????

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