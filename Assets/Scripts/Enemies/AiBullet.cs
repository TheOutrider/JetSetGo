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

    private void OnCollisionEnter(Collision collision)
    {
        if (owner != null && collision.collider.transform.root == owner.root) return; // ignore self-hit
        Debug.Log("AI BULLET COLLIDED WITH " + collision.collider.gameObject.tag + " " + collision.collider.gameObject.name);
        JetHealth health = collision.collider.GetComponentInParent<JetHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit scenery/track geometry (non-trigger solid colliders) -> despawn
        if (!collision.collider.isTrigger)
            Destroy(gameObject);
    }
}