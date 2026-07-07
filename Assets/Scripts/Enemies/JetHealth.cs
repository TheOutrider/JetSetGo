using System;
using UnityEngine;

/// <summary>
/// Generic health component. Attach to both the Player jet and AI Enemy jets.
/// Bullets/Missiles call TakeDamage() on whatever they hit.
/// </summary>
public class JetHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionLifetime = 3f;

    [Header("On Death")]
    [Tooltip("If true, the GameObject is destroyed after death (with destroyDelay).")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 0.15f; // small delay so explosion isn't cut off if it's parented

    public bool IsDead { get; private set; }

    /// <summary>Subscribe to this to react to death (e.g. disable AI control, stop race tracking).</summary>
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log("TAKING DAMAGE : " + amount.ToString());
        if (IsDead || amount <= 0f) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (explosionPrefab)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, explosionLifetime);
        }

        OnDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}