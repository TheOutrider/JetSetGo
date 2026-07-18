using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceModeManager : MonoBehaviour
{
    public static RaceModeManager Instance { get; private set; }

    [Header("Player")]
    public GameObject playerPrefab;
    public Transform playerSpawnTransform;

    [Header("AI Opponents")]
    public GameObject aiPrefab;
    public List<Transform> aiSpawnTransforms = new List<Transform>();
    [Tooltip("Optional friendly names, otherwise 'AI 1', 'AI 2'...")]
    // public List<string> aiNames = new List<string>();
    public List<JetModel> aiEnemies = new List<JetModel>();
    public JetDatabase jetDatabase;

    [Header("Waypoints (AI navigation path)")]
    public WaypointContainer waypointContainer;

    [Header("Checkpoints (race progress / order)")]
    [Tooltip("Assign checkpoints IN ORDER in the Inspector")]
    public List<Checkpoint> checkpoints = new List<Checkpoint>();

    public RaceHUD raceHUD;

    [Header("UI")]
    public TextMeshProUGUI timerText;       // count-up stopwatch, purely informational — no lose condition
    public TextMeshProUGUI checkpointText;  // player's "3/8"
    public TextMeshProUGUI leaderboardText; // live placements
    public GameObject finishPanel, buttonGrid;
    public Image startPanel;
    public TextMeshProUGUI startRaceText;
    public Button restartButton;

    [Header("Countdown")]
    public string[] countdownWords = { "Jet", "Set", "Go" };
    public float wordDisplayDuration = 1f;

    // State
    private float elapsedTime = 0f;
    private bool raceStarted = false;
    private bool raceFinished = false; // true once the player finishes

    private GameObject spawnedPlayer;
    private RacerInfo playerRacer;
    private readonly List<RacerInfo> racers = new List<RacerInfo>();
    private readonly List<RacerInfo> finishOrder = new List<RacerInfo>();

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

        SpawnPlayer();
        SpawnAI();

        if (finishPanel) finishPanel.SetActive(false);
        if (buttonGrid) buttonGrid.SetActive(false);

        SetPanelAlpha(startPanel, 1f);
        SetPanelAlpha(startRaceText, 1f);
        if (startPanel) startPanel.gameObject.SetActive(true);
        if (startRaceText) startRaceText.gameObject.SetActive(true);

        if (raceHUD) raceHUD.playerTransform = spawnedPlayer.transform;

        UpdatePlayerCheckpointUI();
        UpdateLeaderboard();
        StartCoroutine(PreRaceCountdown());
    }

    void Update()
    {
        if (!raceStarted || raceFinished) return;
        elapsedTime += Time.deltaTime;
        if (timerText) timerText.text = CheckpointManager.FormatTime(elapsedTime);
    }

    // ── Spawning ─────────────────────────────────────────────────────────────

    void SpawnPlayer()
    {
        spawnedPlayer = Instantiate(playerPrefab, playerSpawnTransform.position, playerSpawnTransform.rotation);

        playerRacer = spawnedPlayer.GetComponent<RacerInfo>();
        if (!playerRacer) playerRacer = spawnedPlayer.AddComponent<RacerInfo>();
        playerRacer.isPlayer = true;
        playerRacer.racerName = "Player";
        playerRacer.rb = spawnedPlayer.GetComponentInChildren<Rigidbody>();

        FreezeRacer(playerRacer.rb, true);
        racers.Add(playerRacer);
    }

    void SpawnAI()
    {
        for (int i = 0; i < aiSpawnTransforms.Count; i++)
        {
            Transform spawnPoint = aiSpawnTransforms[i];
            if (!spawnPoint) continue;

            GameObject aiObj = Instantiate(aiPrefab, spawnPoint.position, spawnPoint.rotation);

            RacerInfo racer = aiObj.GetComponent<RacerInfo>();
            if (!racer) racer = aiObj.AddComponent<RacerInfo>();
            racer.isPlayer = false;
            racer.racerName = (i < aiEnemies.Count && !string.IsNullOrEmpty(aiEnemies[i].AliasName)) ? aiEnemies[i].AliasName : $"AI {i + 1}";
            racer.rb = aiObj.GetComponentInChildren<Rigidbody>();



            JetAiEnemy pilot = aiObj.GetComponent<JetAiEnemy>();
            if (pilot)
            {
                pilot.waypointContainer = waypointContainer;
                if (!pilot.jetRb) pilot.jetRb = racer.rb;
                pilot.enabled = false; // held until BeginRace(), so it doesn't Start()/steer during countdown
            }
            EnemyJetSpawner enemyJetSpawner = pilot.GetComponent<EnemyJetSpawner>();
            JetData selectedJet = jetDatabase.jets.Find((x) => x.jetName == aiEnemies[i].JetName);
            enemyJetSpawner.SpawnSelectedJet(selectedJet);

            FreezeRacer(racer.rb, true);
            racers.Add(racer);
        }
    }

    void FreezeRacer(Rigidbody rb, bool freeze)
    {
        if (!rb) return;
        if (freeze)
        {
            rb.linearVelocity = Vector3.zero;   // use rb.velocity instead if on an older Unity version
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    // ── Countdown / start ───────────────────────────────────────────────────

    private IEnumerator PreRaceCountdown()
    {
        float totalDuration = countdownWords.Length * wordDisplayDuration;
        float elapsed = 0f;

        for (int i = 0; i < countdownWords.Length; i++)
        {
            if (startRaceText) startRaceText.text = countdownWords[i];

            float wordEnd = Time.time + wordDisplayDuration;
            while (Time.time < wordEnd)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / totalDuration);
                SetPanelAlpha(startPanel, Mathf.Lerp(1f, 0f, t));
                yield return null;
            }
        }

        SetPanelAlpha(startPanel, 0f);
        if (startPanel) startPanel.gameObject.SetActive(false);
        if (startRaceText) startRaceText.gameObject.SetActive(false);

        BeginRace();
    }

    private void BeginRace()
    {
        raceStarted = true;
        raceFinished = false;
        elapsedTime = 0f;

        foreach (var racer in racers)
            FreezeRacer(racer.rb, false);

        foreach (var racer in racers)
        {
            if (racer.isPlayer) continue;
            JetAiEnemy pilot = racer.GetComponent<JetAiEnemy>();
            if (pilot) pilot.enabled = true;
        }
    }

    // ── Race progress ────────────────────────────────────────────────────────

    /// <summary>Called by a Checkpoint when any racer (player or AI) triggers it.</summary>
    public void CheckpointReached(RacerInfo racer, int index)
    {
        if (racer == null || racer.finished) return;
        if (index != racer.nextCheckpointIndex) return; // wrong order, ignore

        racer.nextCheckpointIndex++;

        if (racer.nextCheckpointIndex >= checkpoints.Count)
        {
            racer.finished = true;
            racer.finishTime = elapsedTime;
            finishOrder.Add(racer);
            racer.placement = finishOrder.Count;

            if (racer.isPlayer)
                FinishRace();
        }

        if (racer.isPlayer)
        {
            UpdatePlayerCheckpointUI();
            RefreshCheckpointVisuals();
        }

        UpdateLeaderboard();
    }

    void RefreshCheckpointVisuals()
    {
        foreach (var cp in checkpoints)
            cp.UpdateVisual();
    }

    void FinishRace()
    {
        raceFinished = true;
        Debug.Log($"[RaceMode] Player finished! Time: {CheckpointManager.FormatTime(elapsedTime)}");

        if (finishPanel) finishPanel.SetActive(true);
        StartCoroutine(ShowButtonGridDelayed(3f));
    }

    private IEnumerator ShowButtonGridDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (buttonGrid) buttonGrid.SetActive(true);
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    void UpdatePlayerCheckpointUI()
    {
        if (checkpointText && playerRacer)
            checkpointText.text = $"Checkpoint: {playerRacer.nextCheckpointIndex}/{checkpoints.Count}";
    }

    void UpdateLeaderboard()
    {
        if (!leaderboardText) return;
        // Remove destroyed racers
        // racers.RemoveAll(r => r == null);

        List<RacerInfo> ordered = racers
            .Where(r => r != null)
            .OrderByDescending(r => r.finished)
            .ThenBy(r => r.finished ? r.placement : int.MaxValue)
            .ThenByDescending(r => r.nextCheckpointIndex)
            .ThenBy(DistanceToNextCheckpoint)
            .ToList();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < ordered.Count; i++)
        {
            RacerInfo r = ordered[i];
            string tag = r.isPlayer ? " (You)" : "";
            sb.AppendLine($"{i + 1}. {r.racerName}{tag}");
            JetAiEnemy pilot = ordered[i].GetComponent<JetAiEnemy>();
            if(pilot != null)
            pilot.rankText.text = $"#{i + 1}";
        }
        leaderboardText.text = sb.ToString();
    }

    float DistanceToNextCheckpoint(RacerInfo racer)
    {
        if (racer.finished || checkpoints.Count == 0) return 0f;
        int idx = Mathf.Clamp(racer.nextCheckpointIndex, 0, checkpoints.Count - 1);
        return Vector3.Distance(racer.transform.position, checkpoints[idx].transform.position);
    }

    // ── Restart ──────────────────────────────────────────────────────────────

    public void RestartRace()
    {
        StopAllCoroutines();

        foreach (var racer in racers)
            if (racer) Destroy(racer.gameObject);
        racers.Clear();
        finishOrder.Clear();

        raceStarted = false;
        raceFinished = false;
        elapsedTime = 0f;

        SpawnPlayer();
        SpawnAI();

        foreach (var cp in checkpoints)
            cp.UpdateVisual();

        if (finishPanel) finishPanel.SetActive(false);
        if (buttonGrid) buttonGrid.SetActive(false);

        SetPanelAlpha(startPanel, 1f);
        SetPanelAlpha(startRaceText, 1f);
        if (startPanel) startPanel.gameObject.SetActive(true);
        if (startRaceText) startRaceText.gameObject.SetActive(true);

        if (raceHUD) raceHUD.playerTransform = spawnedPlayer.transform;

        UpdatePlayerCheckpointUI();
        UpdateLeaderboard();
        StartCoroutine(PreRaceCountdown());
    }

    // ── Small helpers ────────────────────────────────────────────────────────

    void SetPanelAlpha(Graphic g, float a)
    {
        if (!g) return;
        Color c = g.color;
        c.a = a;
        g.color = c;
    }

    // ── Public helpers used by Checkpoint.cs ────────────────────────────────

    public int GetPlayerNextCheckpointIndex() => playerRacer != null ? playerRacer.nextCheckpointIndex : 0;
    public float GetElapsedTime() => elapsedTime;
    public bool IsRaceFinished() => raceFinished;
}