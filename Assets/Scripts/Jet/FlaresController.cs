using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlaresController : MonoBehaviour
{
    [Header("Flare Settings")]
    public GameObject flarePrefab;         // The flare prefab to spawn
    public Transform[] flareSpawnPoints;   // Spawn points on the jet (left/right exhaust)
    public int maxFlares = 10;
    public float deployInterval = 0.15f;   // Time between each flare in a burst
    public int flaresPerBurst = 3;         // How many flares per button press

    [Header("UI")]
    public TextMeshProUGUI flareCountText;
    public Button deployButton;
    public Image buttonCooldownFill;       // Optional: radial fill for cooldown

    [Header("Cooldown")]
    public float cooldownDuration = 3f;

    private int currentFlares;
    private bool isOnCooldown = false;

    void Start()
    {
        currentFlares = maxFlares;
        UpdateUI();

        // Wire button — or wire it via Inspector
        if (deployButton != null)
            deployButton.onClick.AddListener(DeployFlares);
    }

    public void DeployFlares()
    {
        if (isOnCooldown || currentFlares <= 0) return;

        int flaresToDeploy = Mathf.Min(flaresPerBurst, currentFlares);
        StartCoroutine(SpawnFlareBurst(flaresToDeploy));
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator SpawnFlareBurst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Alternate between spawn points if multiple exist
            Transform spawnPoint = flareSpawnPoints[i % flareSpawnPoints.Length];

            GameObject flare = Instantiate(flarePrefab, spawnPoint.position, spawnPoint.rotation);

            // Pass jet's velocity so flares inherit momentum
            Rigidbody jetRb = GetComponent<Rigidbody>();
            if (jetRb != null)
            {
                FlareProjectile fp = flare.GetComponent<FlareProjectile>();
                if (fp != null) fp.inheritedVelocity = jetRb.linearVelocity;
            }

            currentFlares--;
            UpdateUI();
            yield return new WaitForSeconds(deployInterval);
        }
    }

    IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        deployButton.interactable = false;
        float elapsed = 0f;

        while (elapsed < cooldownDuration)
        {
            elapsed += Time.deltaTime;
            if (buttonCooldownFill != null)
                buttonCooldownFill.fillAmount = elapsed / cooldownDuration;
            yield return null;
        }

        isOnCooldown = false;
        if (currentFlares > 0)
            deployButton.interactable = true;

        if (buttonCooldownFill != null)
            buttonCooldownFill.fillAmount = 1f;
    }

    void UpdateUI()
    {
        if (flareCountText != null)
            flareCountText.text = $"FLARES: {currentFlares}/{maxFlares}";

        if (deployButton != null && currentFlares <= 0)
            deployButton.interactable = false;
    }
}