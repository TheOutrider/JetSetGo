using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Animates the game title "Ready Steady Go" letter by letter flying in
/// from the right, then fades in a "Tap to Start" prompt.
///
/// SETUP REQUIREMENTS (see README comments at bottom of file):
///   - One TMP_Text component per word (ReadyText, SteadyText, GoText)
///   - One TMP_Text component for the tap prompt (TapToStartText)
///   - All Text objects must be children of a Screen Space - Overlay Canvas
/// </summary>
public class TitleScreenAnimator : MonoBehaviour
{
    // ── Word References ────────────────────────────────────────
    [Header("Title Word References")]
    [Tooltip("TMP Text showing 'Ready'")]
    public TMP_Text readyText;

    [Tooltip("TMP Text showing 'Steady'")]
    public TMP_Text steadyText;

    [Tooltip("TMP Text showing 'Go'")]
    public TMP_Text goText;

    public Button startButton;

    [SerializeField] private string mainMenuScene = "MainMenu";

    // ── Tap Prompt ─────────────────────────────────────────────
    [Header("Tap To Start")]
    [Tooltip("TMP Text showing 'Tap to Start'")]
    public TMP_Text tapToStartText;

    // ── Fly-In Settings ────────────────────────────────────────
    [Header("Fly-In Settings")]
    [Tooltip("How far off-screen to the right each word starts (in pixels).")]
    public float startOffsetX = 1400f;

    [Tooltip("How long each word takes to reach its final position (seconds).")]
    public float flyDuration = 0.35f;

    [Tooltip("Gap between each word starting its animation (seconds).")]
    public float delayBetweenWords = 0.15f;

    // ── Tap Prompt Settings ────────────────────────────────────
    [Header("Tap Prompt Settings")]
    [Tooltip("How long the fade-in/out cycle takes (seconds).")]
    public float fadeCycleDuration = 1.2f;

    [Tooltip("How long to wait after the last word lands before showing the prompt.")]
    public float delayAfterTitle = 0.6f;

    // ── Internals ──────────────────────────────────────────────
    private Vector2 _readyAnchor;
    private Vector2 _steadyAnchor;
    private Vector2 _goAnchor;

    void Start()
    {
        ValidateReferences();

        // Cache each word's final resting position
        _readyAnchor  = readyText.rectTransform.anchoredPosition;
        _steadyAnchor = steadyText.rectTransform.anchoredPosition;
        _goAnchor     = goText.rectTransform.anchoredPosition;

        // Move all words off-screen to the right instantly
        SetOffscreen(readyText,  _readyAnchor);
        SetOffscreen(steadyText, _steadyAnchor);
        SetOffscreen(goText,     _goAnchor);

        // Hide tap prompt completely
        SetAlpha(tapToStartText, 0f);

        StartCoroutine(PlayTitleSequence());
    }

    // ── Main Sequence ──────────────────────────────────────────

    IEnumerator PlayTitleSequence()
    {
        // Fly in each word one after another
        yield return StartCoroutine(FlyIn(readyText,  _readyAnchor));
        yield return new WaitForSeconds(delayBetweenWords);

        yield return StartCoroutine(FlyIn(steadyText, _steadyAnchor));
        yield return new WaitForSeconds(delayBetweenWords);

        yield return StartCoroutine(FlyIn(goText,     _goAnchor));

        // Short pause before showing tap prompt
        yield return new WaitForSeconds(delayAfterTitle);

        // Start the looping fade
        StartCoroutine(FadeLoop(tapToStartText));
    }

    // ── Fly-In Coroutine ───────────────────────────────────────

    IEnumerator FlyIn(TMP_Text word, Vector2 finalPos)
    {
        RectTransform rt   = word.rectTransform;
        Vector2       start = new Vector2(finalPos.x + startOffsetX, finalPos.y);
        float         elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / flyDuration);
            float eased   = EaseOutCubic(t);          // fast start, smooth landing
            rt.anchoredPosition = Vector2.Lerp(start, finalPos, eased);
            yield return null;
        }

        rt.anchoredPosition = finalPos; // guarantee exact landing
    }

    // ── Fade Loop Coroutine ────────────────────────────────────

    IEnumerator FadeLoop(TMP_Text label)
    {
        float halfCycle = fadeCycleDuration * 0.5f;

        while (true)
        {
            if (startButton.onClick.GetPersistentEventCount() > 0) 
            {
                Debug.Log("Button already has persistent listeners from the Inspector.");
            } else
            {
                startButton.onClick.AddListener(LoadMainMenuScene);
            }

           
            // Fade IN
            yield return StartCoroutine(FadeTo(label, 0f, 1f, halfCycle));

            // Fade OUT
            yield return StartCoroutine(FadeTo(label, 1f, 0f, halfCycle));
        }
    }

    IEnumerator FadeTo(TMP_Text label, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(label, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetAlpha(label, to);
    }

    // ── Helpers ────────────────────────────────────────────────

    void SetOffscreen(TMP_Text word, Vector2 anchor)
    {
        word.rectTransform.anchoredPosition = new Vector2(anchor.x + startOffsetX, anchor.y);
    }

    void SetAlpha(TMP_Text label, float alpha)
    {
        Color c = label.color;
        c.a = alpha;
        label.color = c;
    }

    /// Decelerates quickly — gives the "slam into place" feel
    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    void ValidateReferences()
    {
        if (readyText    == null) Debug.LogError("[TitleScreenAnimator] ReadyText is not assigned.",       this);
        if (steadyText   == null) Debug.LogError("[TitleScreenAnimator] SteadyText is not assigned.",      this);
        if (goText       == null) Debug.LogError("[TitleScreenAnimator] GoText is not assigned.",          this);
        if (tapToStartText == null) Debug.LogError("[TitleScreenAnimator] TapToStartText is not assigned.", this);
    }

    private void LoadMainMenuScene()
    {
        Debug.Log("LOADING MAIN MENU");
        SceneManager.LoadScene(mainMenuScene);
    }

    /*
    ════════════════════════════════════════════════════════════════
     SETUP GUIDE
    ════════════════════════════════════════════════════════════════

    1. CANVAS
       - Create a Canvas (GameObject > UI > Canvas)
       - Render Mode: Screen Space - Overlay
       - Add a CanvasScaler, set to "Scale With Screen Size"
         Reference Resolution: 1920 x 1080 (or your target)

    2. TITLE WORDS  (one TMP_Text per word)
       - Under the Canvas, create 3 UI > Text - TextMeshPro objects:
           ReadyText   → text: "Ready"
           SteadyText  → text: "Steady"
           GoText      → text: "Go"
       - Arrange them horizontally in the center of the canvas
         at whatever size and position looks good to you.
       - The script saves their position on Start() as the landing target,
         so position them exactly where you want them to END UP.

    3. TAP PROMPT
       - Create one more TMP_Text: TapToStartText → text: "Tap to Start"
       - Position it near the bottom of the canvas.
       - Set its alpha to 0 in the Inspector (Color field) — the script
         controls alpha at runtime, but it's cleaner to start invisible.

    4. ATTACH THE SCRIPT
       - Create an empty GameObject in the scene (name it "TitleAnimator")
       - Add this script to it
       - Drag each TMP_Text into the matching slot in the Inspector

    5. TWEAK THE VALUES
       Fly-In Settings:
         startOffsetX      → increase if words start too close to center
         flyDuration       → lower = faster slam (try 0.25 – 0.4)
         delayBetweenWords → gap between each word launching (try 0.1 – 0.3)

       Tap Prompt Settings:
         fadeCycleDuration → total time for one full fade in+out cycle
         delayAfterTitle   → pause before the prompt appears

    ════════════════════════════════════════════════════════════════
    */
}