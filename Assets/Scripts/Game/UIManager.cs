using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager — Persistent Singleton (DontDestroyOnLoad).
///
/// The RaceCanvas lives in scene 2, so it cannot be wired in the Inspector here.
/// Instead, attach RaceCanvasRegistrar.cs to the RaceCanvas GameObject in scene 2.
/// It will call UIManager.RegisterRaceCanvas() on Awake, which stores the
/// references and immediately shows the waiting state.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePanelRegistry();
    }

    // ─────────────────────────────────────────────
    //  Panel Registry
    // ─────────────────────────────────────────────

    [Header("Panels (assign in Inspector or register at runtime)")]
    [SerializeField] private List<UIPanel> panels = new();

    private readonly Dictionary<string, GameObject> _panelMap = new();
    private readonly Stack<string> _panelHistory = new();

    private void InitializePanelRegistry()
    {
        foreach (var entry in panels)
            if (entry.panelObject != null)
                _panelMap[entry.panelId] = entry.panelObject;
    }

    public void RegisterPanel(string panelId, GameObject panelObject)
    {
        _panelMap[panelId] = panelObject;
        Debug.Log($"[UIManager] Registered panel: {panelId}");
    }

    // ─────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────

    public void ShowPanel(string panelId, bool addToHistory = true)
    {
        if (!TryGetPanel(panelId, out var panel)) return;
        panel.SetActive(true);
        if (addToHistory) _panelHistory.Push(panelId);
        Debug.Log($"[UIManager] Showing panel: {panelId}");
    }

    public void HidePanel(string panelId)
    {
        if (!TryGetPanel(panelId, out var panel)) return;
        panel.SetActive(false);
        Debug.Log($"[UIManager] Hiding panel: {panelId}");
    }

    public void HideAllPanels()
    {
        foreach (var kvp in _panelMap) kvp.Value.SetActive(false);
        _panelHistory.Clear();
    }

    public void SwitchToPanel(string panelId)
    {
        HideAllPanels();
        ShowPanel(panelId);
    }

    public void GoBack()
    {
        if (_panelHistory.Count == 0) return;
        var current = _panelHistory.Pop();
        HidePanel(current);
        if (_panelHistory.TryPeek(out var previous))
            ShowPanel(previous, addToHistory: false);
    }

    public bool IsPanelActive(string panelId) =>
        _panelMap.TryGetValue(panelId, out var p) && p.activeSelf;

    // ─────────────────────────────────────────────
    //  Race Canvas
    //
    //  DO NOT assign these in the Inspector — they live in a different scene.
    //  RaceCanvasRegistrar (on the RaceCanvas GameObject in scene 2) calls
    //  RegisterRaceCanvas() on its Awake, which fills these in at runtime.
    // ─────────────────────────────────────────────

    // Runtime references — populated by RegisterRaceCanvas()
    private GameObject _raceCanvasRoot;
    private TMP_Text _raceTitleText;
    private TMP_Text _raceDescText;

    [Header("Race Canvas — Waiting copy (shown before jet spawns)")]
    [SerializeField] private string waitingTitle = "Get Ready";
    [SerializeField] private string waitingDescription = "Waiting for your jet to spawn…";

    [Header("Race Canvas — Ready copy (shown after jet spawns)")]
    [SerializeField] private string readyTitle = "Race!";
    [SerializeField] private string readyDescription = "Good luck, pilot!";

    private Coroutine _hideRaceCanvasCoroutine;

    /// <summary>
    /// Called by RaceCanvasRegistrar.Awake() when scene 2 loads.
    /// Stores the references and immediately shows the waiting state.
    /// </summary>
    public void RegisterRaceCanvas(GameObject root, TMP_Text titleText, TMP_Text descText)
    {
        _raceCanvasRoot = root;
        _raceTitleText = titleText;
        _raceDescText = descText;

        Debug.Log("[UIManager] RaceCanvas registered.");

        // Show the waiting state the moment the canvas is available
        ShowRaceCanvasWaiting();
    }

    /// <summary>
    /// Shows the RaceCanvas with the "waiting for spawn" copy.
    /// Safe to call before RegisterRaceCanvas() — it will just log a warning.
    /// </summary>
    public void ShowRaceCanvasWaiting()
    {
        if (_raceCanvasRoot == null)
        {
            Debug.LogWarning("[UIManager] RaceCanvas not registered yet — " +
                             "make sure RaceCanvasRegistrar is on the RaceCanvas GameObject.");
            return;
        }

        if (_hideRaceCanvasCoroutine != null)
        {
            StopCoroutine(_hideRaceCanvasCoroutine);
            _hideRaceCanvasCoroutine = null;
        }

        _raceCanvasRoot.SetActive(true);
        if (_raceTitleText != null) _raceTitleText.text = waitingTitle;
        if (_raceDescText != null) _raceDescText.text = waitingDescription;
    }

    /// <summary>
    /// Call this from PlayerJet.OnSpawned() (owner only).
    /// Swaps to the ready copy, then hides the canvas after hideDelay seconds.
    /// </summary>
    public void OnLocalJetSpawned(float hideDelay = 2f)
    {
        if (_raceCanvasRoot == null)
        {
            Debug.LogWarning("[UIManager] RaceCanvas not registered — cannot show spawn state.");
            return;
        }

        if (_hideRaceCanvasCoroutine != null)
            StopCoroutine(_hideRaceCanvasCoroutine);

        _raceCanvasRoot.SetActive(true);
        if (_raceTitleText != null) _raceTitleText.text = readyTitle;
        if (_raceDescText != null) _raceDescText.text = readyDescription;

        if (hideDelay <= 0f)
            _raceCanvasRoot.SetActive(false);
        else
            _hideRaceCanvasCoroutine = StartCoroutine(HideRaceCanvasAfterDelay(hideDelay));
    }

    private System.Collections.IEnumerator HideRaceCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_raceCanvasRoot != null) _raceCanvasRoot.SetActive(false);
        _hideRaceCanvasCoroutine = null;
    }

    // ─────────────────────────────────────────────
    //  Loading Screen
    // ─────────────────────────────────────────────

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingText;

    public void ShowLoadingScreen(string message = "Loading...")
    {
        if (loadingScreen == null) return;
        loadingScreen.SetActive(true);
        SetLoadingProgress(0f, message);
    }

    public void HideLoadingScreen()
    {
        if (loadingScreen == null) return;
        loadingScreen.SetActive(false);
    }

    public void SetLoadingProgress(float progress, string message = null)
    {
        if (loadingBar != null) loadingBar.value = Mathf.Clamp01(progress);
        if (loadingText != null && message != null) loadingText.text = message;
    }

    // ─────────────────────────────────────────────
    //  HUD
    // ─────────────────────────────────────────────

    [Header("HUD Elements")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthBar;

    public void SetScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void SetHealth(float current, float max)
    {
        if (healthText != null) healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        if (healthBar != null) healthBar.value = Mathf.Clamp01(current / max);
    }

    // ─────────────────────────────────────────────
    //  Popup / Dialog
    // ─────────────────────────────────────────────

    [Header("Popup Dialog")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text popupTitle;
    [SerializeField] private TMP_Text popupMessage;
    [SerializeField] private Button popupConfirmButton;
    [SerializeField] private Button popupCancelButton;
    [SerializeField] private TMP_Text popupConfirmLabel;
    [SerializeField] private TMP_Text popupCancelLabel;

    public void ShowPopup(
        string title,
        string message,
        string confirmLabel = "OK",
        System.Action onConfirm = null,
        string cancelLabel = null,
        System.Action onCancel = null)
    {
        if (popupPanel == null) return;

        popupPanel.SetActive(true);
        if (popupTitle != null) popupTitle.text = title;
        if (popupMessage != null) popupMessage.text = message;

        if (popupConfirmButton != null)
        {
            popupConfirmButton.gameObject.SetActive(true);
            if (popupConfirmLabel != null) popupConfirmLabel.text = confirmLabel;
            popupConfirmButton.onClick.RemoveAllListeners();
            popupConfirmButton.onClick.AddListener(() => { onConfirm?.Invoke(); HidePopup(); });
        }

        if (popupCancelButton != null)
        {
            bool show = cancelLabel != null;
            popupCancelButton.gameObject.SetActive(show);
            if (show)
            {
                if (popupCancelLabel != null) popupCancelLabel.text = cancelLabel;
                popupCancelButton.onClick.RemoveAllListeners();
                popupCancelButton.onClick.AddListener(() => { onCancel?.Invoke(); HidePopup(); });
            }
        }
    }

    public void HidePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────
    //  Toast / Notification
    // ─────────────────────────────────────────────

    [Header("Toast Notification")]
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TMP_Text toastText;

    private Coroutine _toastCoroutine;

    public void ShowToast(string message, float duration = 2.5f)
    {
        if (toastPanel == null) return;
        if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(ToastRoutine(message, duration));
    }

    private System.Collections.IEnumerator ToastRoutine(string message, float duration)
    {
        if (toastText != null) toastText.text = message;
        toastPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        toastPanel.SetActive(false);
        _toastCoroutine = null;
    }

    // ─────────────────────────────────────────────
    //  Cursor Utility
    // ─────────────────────────────────────────────

    public void ShowCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────

    private bool TryGetPanel(string panelId, out GameObject panel)
    {
        if (_panelMap.TryGetValue(panelId, out panel)) return true;
        Debug.LogWarning($"[UIManager] Panel not found: '{panelId}'.");
        return false;
    }
}

[System.Serializable]
public class UIPanel
{
    public string panelId;
    public GameObject panelObject;
}