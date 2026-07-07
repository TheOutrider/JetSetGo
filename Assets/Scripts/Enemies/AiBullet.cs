using UnityEngine;

/// <summary>
/// Straight-flying bullet. Fire point's forward direction determines travel direction.
/// Requires a trigger Collider on the bullet prefab (e.g. small SphereCollider, isTrigger = true)
/// and a Rigidbody is NOT required (movement is done manually in Update).
/// </summary>
public class AiBullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 90f;
    public float lifeTime = 4f;

    [Header("Damage")]
    public float damage = 10f;

    [Header("Owner")]
    [Tooltip("Assign the shooter's transform so a bullet can't damage its own jet.")]
    public Transform owner;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.root == owner.root) return; // ignore self-hit
    Debug.Log("AI BULLET COLLIDED WITH " + other.gameObject.tag + " " + other.gameObject.name);
        JetHealth health = other.GetComponentInParent<JetHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit scenery/track geometry (non-trigger solid colliders) -> despawn
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}