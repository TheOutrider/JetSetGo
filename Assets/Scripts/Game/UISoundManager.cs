using UnityEngine;
using UnityEngine.UI;

public class UISoundManager : MonoBehaviour
{
    // public static UISoundManager Instance { get; private set; }

    [Header("Audio")]
    public AudioClip universalButtonClickSound;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // Instance = this;
        // DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        RegisterAllButtons();
    }

    public void RegisterAllButtons()
    {
        foreach (Button btn in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            RegisterButton(btn);
    }

    public void RegisterButton(Button btn)
    {
        // Avoid double-registering
        btn.onClick.RemoveListener(PlayClickSound);
        btn.onClick.AddListener(PlayClickSound);
    }

    public void PlayClickSound()
    {
        if (universalButtonClickSound)
            audioSource.PlayOneShot(universalButtonClickSound, volume);
    }
}