using UnityEngine;

public class MissileOffline : MonoBehaviour
{
    [SerializeField] private float speed = 300f;
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float damage = 100f;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private LayerMask hitLayer;

    private Vector3 targetPoint;
    private bool isInitialized = false;

    private AudioSource missileAudioSource;
    [SerializeField] private AudioClip explosionSound;

    private void Awake()
    {
        missileAudioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Vector3 target)
    {
        targetPoint = target;
        isInitialized = true;

        missileAudioSource = GetComponent<AudioSource>();

        if (missileAudioSource != null)
            missileAudioSource.Play();

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
        Explode(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered with " + other.gameObject.name);
        Explode(transform.position);
    }

    void Explode(Vector3 point)
    {
        if (explosionVFX != null)
            Instantiate(explosionVFX, point, Quaternion.identity);

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(
                explosionSound,
                point,
                1f
            );
        }

        Collider[] hits = Physics.OverlapSphere(point, explosionRadius, hitLayer);

        foreach (var col in hits)
        {
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