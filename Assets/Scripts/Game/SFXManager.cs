using UnityEngine;
using UnityEngine.UI;

public class SFXManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip buttonClickSound;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        
    }

    public void PlayClickSound()
    {
        if (buttonClickSound)
            audioSource.PlayOneShot(buttonClickSound, volume);
    }
}