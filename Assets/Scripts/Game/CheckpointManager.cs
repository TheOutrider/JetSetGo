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

    [Header("Checkpoints")]
    [Tooltip("Assign checkpoints IN ORDER in the Inspector")]
    public List<Checkpoint> checkpoints = new List<Checkpoint>();

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI checkpointText;
    public GameObject finishPanel;
    public TextMeshProUGUI finalTimeText;

    [Header("Settings")]
    public bool countUp = true; // true = stopwatch, false = countdown
    public float countdownStartTime = 60f; // only used if countUp = false

    // State
    private float elapsedTime = 0f;
    private int nextCheckpointIndex = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-collect checkpoints if not assigned manually
        if (checkpoints.Count == 0)
        {
            checkpoints.AddRange(FindObjectsByType<Checkpoint>(FindObjectsSortMode.None));
            checkpoints.Sort((a, b) => a.checkpointIndex.CompareTo(b.checkpointIndex));
        }

        Instantiate(playerPrefab, playerStartTransform.position, playerStartTransform.rotation);

        // Number them
        for (int i = 0; i < checkpoints.Count; i++)
            checkpoints[i].checkpointIndex = i;

        if (finishPanel) finishPanel.SetActive(false);
        UpdateCheckpointUI();
        StartRace();
    }

    void Update()
    {
        if (!raceStarted || raceFinished) return;

        if (countUp)
            elapsedTime += Time.deltaTime;
        else
        {
            elapsedTime -= Time.deltaTime;
            if (elapsedTime <= 0f) { elapsedTime = 0f; FinishRace(); }
        }

        UpdateTimerUI();
    }

    public void StartRace()
    {
        raceStarted = true;
        raceFinished = false;
        elapsedTime = countUp ? 0f : countdownStartTime;
        nextCheckpointIndex = 0;
        UpdateCheckpointUI();
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

    void FinishRace()
    {
        raceFinished = true;
        string timeStr = FormatTime(elapsedTime);
        Debug.Log($"[Race] Finished! Time: {timeStr}");

        if (finishPanel)
        {
            finishPanel.SetActive(true);
            if (finalTimeText) finalTimeText.text = $"Your Time: {timeStr}";
        }
    }

    // ── UI helpers ──────────────────────────────────────────────────────────

    void UpdateTimerUI()
    {
        if (timerText) timerText.text = FormatTime(elapsedTime);
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

    // ── Public helpers ───────────────────────────────────────────────────────

    public float GetElapsedTime() => elapsedTime;
    public bool IsRaceFinished() => raceFinished;
    public int GetNextCheckpointIndex() => nextCheckpointIndex;

    /// <summary>Call this (e.g. from a UI button) to restart the race.</summary>
    public void RestartRace()
    {
        foreach (var cp in checkpoints) cp.SetPassed(false);
        if (finishPanel) finishPanel.SetActive(false);
        StartRace();
    }
}