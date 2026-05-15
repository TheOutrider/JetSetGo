using UnityEngine;
using PurrNet;

public class Missile : NetworkBehaviour
{
    [SerializeField] private float speed = 300f;
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float damage = 100f;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private LayerMask hitLayer;

    private Vector3 targetPoint;
    private bool isInitialized = false;

    public void Initialize(Vector3 target)
    {
        targetPoint = target;
        isInitialized = true;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;

        // Fly straight toward the initial aim point
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);
        //Explode(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered with " + other.gameObject.name);
        //Explode(transform.position);
    }

    void Explode(Vector3 point)
    {
        // Spawn VFX
        if (explosionVFX != null)
            Instantiate(explosionVFX, point, Quaternion.identity);

        // Apply splash damage to everything in radius
        Collider[] hits = Physics.OverlapSphere(point, explosionRadius, hitLayer);
        foreach (var col in hits)
        {
            // Hook into your health/damage system here
            // e.g. col.GetComponent<JetHealth>()?.TakeDamage(damage);
            Debug.Log($"Missile hit: {col.gameObject.name} for {damage} damage");
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}