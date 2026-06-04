using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Player Spawner")]
    public GameObject playerPrefab;
    public Transform playerStartTransform;

    public RaceHUD raceHUD;

    [Header("Checkpoints")]
    [Tooltip("Assign checkpoints IN ORDER in the Inspector")]
    public List<Checkpoint> checkpoints = new List<Checkpoint>();

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI checkpointText;
    public GameObject finishPanel, lostPanel, buttonGrid;
    public Image startPanel;
    public TextMeshProUGUI finalTimeText, startRaceText;
    public Button restartButton;

    [Header("Settings")]
    public bool countUp = false;
    public float countdownStartTime = 60f;
    public float warningTime = 10f;       // seconds left before timer turns red

    [Header("Countdown")]
    [Tooltip("Words shown during the pre-race countdown, one per second")]
    public string[] countdownWords = { "Jet", "Set", "Go" };
    public float wordDisplayDuration = 1f;

    // State
    private float elapsedTime = 0f;
    private int nextCheckpointIndex = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private GameObject spawnedPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (checkpoints.Count == 0)
        {
            checkpoints.AddRange(FindObjectsByType<Checkpoint>(FindObjectsSortMode.None));
            checkpoints.Sort((a, b) => a.checkpointIndex.CompareTo(b.checkpointIndex));
        }

        for (int i = 0; i < checkpoints.Count; i++)
            checkpoints[i].checkpointIndex = i;

        spawnedPlayer = Instantiate(playerPrefab, playerStartTransform.position, playerStartTransform.rotation);

        if (finishPanel) finishPanel.SetActive(false);
        if (lostPanel) lostPanel.SetActive(false);
        if (buttonGrid) buttonGrid.SetActive(false);

        if (startPanel)
        {
            Color c = startPanel.color;
            c.a = 1f;
            startPanel.color = c;
        }
        if (startRaceText)
        {
            Color c = startRaceText.color;
            c.a = 1f;
            startRaceText.color = c;
        }

        raceHUD.playerTransform = spawnedPlayer.transform;

        UpdateCheckpointUI();
        StartCoroutine(PreRaceCountdown());
    }

    void Update()
    {
        if (!raceStarted || raceFinished) return;

        if (countUp)
            elapsedTime += Time.deltaTime;
        else
        {
            elapsedTime -= Time.deltaTime;
            if (elapsedTime <= 0f) { elapsedTime = 0f; LoseRace(); }
        }

        UpdateTimerUI();
    }

    // ── Pre-race countdown ───────────────────────────────────────────────────

    private IEnumerator PreRaceCountdown()
    {
        float totalDuration = countdownWords.Length * wordDisplayDuration;
        float elapsed = 0f;

        for (int i = 0; i < countdownWords.Length; i++)
        {
            // Show the word
            if (startRaceText) startRaceText.text = countdownWords[i];

            float wordStart = Time.time;
            float wordEnd = wordStart + wordDisplayDuration;

            // Fade the startPanel alpha over the full countdown, word by word slice
            while (Time.time < wordEnd)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / totalDuration);

                if (startPanel)
                {
                    Color c = startPanel.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    startPanel.color = c;
                }

                yield return null;
            }
        }

        // Ensure fully transparent and hide both objects
        if (startPanel)
        {
            Color c = startPanel.color;
            c.a = 0f;
            startPanel.color = c;
            startPanel.gameObject.SetActive(false);
        }
        if (startRaceText)
            startRaceText.gameObject.SetActive(false);

        BeginRace();
    }

    // ── Race lifecycle ───────────────────────────────────────────────────────

    private void BeginRace()
    {
        raceStarted = true;
        raceFinished = false;
        elapsedTime = countUp ? 0f : countdownStartTime;
        nextCheckpointIndex = 0;
        UpdateCheckpointUI();
    }

    public void StartRace()
    {
        // Public entry kept for external callers; resets and kicks off countdown
        foreach (var cp in checkpoints) cp.SetPassed(false);

        if (finishPanel) finishPanel.SetActive(false);
        if (lostPanel) lostPanel.SetActive(false);
        if (buttonGrid) buttonGrid.SetActive(false);

        // Reset panel visuals
        if (startPanel)
        {
            Color c = startPanel.color;
            c.a = 1f;
            startPanel.color = c;
            startPanel.gameObject.SetActive(true);
        }
        if (startRaceText)
        {
            Color c = startRaceText.color;
            c.a = 1f;
            startRaceText.color = c;
            startRaceText.gameObject.SetActive(true);
        }

        raceStarted = false;
        raceFinished = false;

        StartCoroutine(PreRaceCountdown());
    }

    /// <summary>Called by a Checkpoint when the player triggers it.</summary>
    public void CheckpointReached(int index)
    {
        if (raceFinished) return;

        if (index != nextCheckpointIndex)
        {
            Debug.Log($"[Checkpoints] Wrong order! Expected {nextCheckpointIndex}, got {index}");
            return;
        }

        checkpoints[index].SetPassed(true);
        nextCheckpointIndex++;

        Debug.Log($"[Checkpoints] Checkpoint {index + 1}/{checkpoints.Count} reached!");

        UpdateCheckpointUI();

        if (nextCheckpointIndex >= checkpoints.Count)
            FinishRace();
    }

    private void FinishRace()
    {
        raceFinished = true;
        string timeStr = FormatTime(elapsedTime);
        Debug.Log($"[Race] Finished! Time: {timeStr}");

        if (finishPanel)
        {
            finishPanel.SetActive(true);
            if (finalTimeText) finalTimeText.text = $"Your Time: {timeStr}";
        }

        // Show button grid after a 3-second delay
        StartCoroutine(ShowButtonGridDelayed(3f));
    }

    private void LoseRace()
    {
        raceFinished = true;
        Debug.Log("[Race] Time ran out — lost!");

        if (lostPanel) lostPanel.SetActive(true);

        // Show button grid immediately on loss
        if (buttonGrid) buttonGrid.SetActive(true);
    }

    private IEnumerator ShowButtonGridDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (buttonGrid) buttonGrid.SetActive(true);
    }

    // ── Restart ──────────────────────────────────────────────────────────────

    /// <summary>Wired to the Restart button via AddListener in Start().</summary>
    public void RestartRace()
    {
        StopAllCoroutines();

        foreach (var cp in checkpoints) cp.SetPassed(false);

        if (finishPanel) finishPanel.SetActive(false);
        if (lostPanel) lostPanel.SetActive(false);
        if (buttonGrid) buttonGrid.SetActive(false);
        // Reset timer color in case it was red from warning
        if (timerText) timerText.color = Color.white;

        // Reset start panel visuals and make them visible again
        if (startPanel)
        {
            Color c = startPanel.color;
            c.a = 1f;
            startPanel.color = c;
            startPanel.gameObject.SetActive(true);
        }
        if (startRaceText)
        {
            Color c = startRaceText.color;
            c.a = 1f;
            startRaceText.color = c;
            startRaceText.gameObject.SetActive(true);
        }

        raceStarted = false;
        raceFinished = false;

        StartCoroutine(PreRaceCountdown());
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    void UpdateTimerUI()
    {
        if (!timerText) return;

        if (countUp)
        {
            timerText.text = FormatTime(elapsedTime);
            timerText.color = Color.white;
        }
        else
        {
            // Show whole seconds counting down: 60, 59, 58 ... 1, 0
            int secondsLeft = Mathf.CeilToInt(elapsedTime);
            timerText.text = secondsLeft.ToString();
            timerText.color = secondsLeft <= warningTime ? Color.red : Color.white;
        }
    }

    void UpdateCheckpointUI()
    {
        if (checkpointText)
            checkpointText.text = $"Checkpoint: {nextCheckpointIndex}/{checkpoints.Count}";
    }

    public static string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60f);
        int sec = Mathf.FloorToInt(t % 60f);
        int ms = Mathf.FloorToInt((t * 100f) % 100f);
        return $"{min:00}:{sec:00}.{ms:00}";
    }

    // ── Public helpers ────────────────────────────────────────────────────────

    public float GetElapsedTime() => elapsedTime;
    public bool IsRaceFinished() => raceFinished;
    public int GetNextCheckpointIndex() => nextCheckpointIndex;
}