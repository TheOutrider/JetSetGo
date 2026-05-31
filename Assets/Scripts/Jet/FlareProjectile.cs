//using System;
using System.Collections;
using UnityEngine;

public class FlareProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float ejectionForce = 8f;       // Force pushing flare away from jet
    public float lifetime = 4f;            // How long before it disappears
    public float drag = 1.5f;             // Air resistance

    [Header("Visual")]
    public Light flareLight;               // Point light on the flare
    public ParticleSystem smokeTrail;
    public ParticleSystem sparkBurst;
    public AnimationCurve lightIntensityCurve; // Flicker over lifetime

    [HideInInspector]
    public Vector3 inheritedVelocity;      // Set by FlareManager

    private Rigidbody rb;
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = drag;
        spawnTime = Time.time;

        // Inherit jet momentum + add ejection force sideways/downward
        Vector3 ejectDir = (-transform.up + -transform.right * 0.5f).normalized;
        rb.linearVelocity = inheritedVelocity + ejectDir * ejectionForce;

        // Add slight random spread
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

        StartCoroutine(FlareLifetime());
    }

    void Update()
    {
        // Animate light flicker
        if (flareLight != null && lightIntensityCurve != null)
        {
            float t = (Time.time - spawnTime) / lifetime;
            flareLight.intensity = lightIntensityCurve.Evaluate(t) * 3f
                                   + Mathf.Sin(Time.time * 20f) * 0.3f; // flicker
        }
    }

    IEnumerator FlareLifetime()
    {
        // Burn bright phase
        yield return new WaitForSeconds(lifetime * 0.75f);

        // Fade out
        float fadeTime = lifetime * 0.25f;
        float elapsed = 0f;
        Color startColor = flareLight != null ? flareLight.color : Color.white;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);
            if (flareLight != null) flareLight.intensity *= alpha;
            yield return null;
        }

        // Stop particles, then destroy
        if (smokeTrail != null) smokeTrail.Stop();
        if (sparkBurst != null) sparkBurst.Stop();
        yield return new WaitForSeconds(1f); // Let particles finish
        Destroy(gameObject);
    }
}