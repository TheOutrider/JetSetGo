using UnityEngine;

/// <summary>
/// Homing missile. Set .target after Instantiate to have it track and steer toward a jet.
/// Requires a trigger Collider on the missile prefab.
/// </summary>
public class AiMissile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 55f;
    public float turnSpeed = 110f; // degrees/sec - how sharply it can correct course
    public float lifeTime = 6f;

    [Header("Damage")]
    public float damage = 40f;

    [Header("Owner / Target")]
    public Transform owner;
    public Transform target;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target)
        {
            Vector3 toTarget = target.position - transform.position;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (owner != null && collision.collider.transform.root == owner.root) return; // ignore self-hit
        Debug.Log("AI MISSILE  COLLIDED WITH " + collision.collider.gameObject.tag + " " + collision.collider.gameObject.name);
        JetHealth health = collision.collider.GetComponentInParent<JetHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // if (!collision.collider.isTrigger)
        if(collision.gameObject.tag != "Checkpoint")
            Destroy(gameObject);
    }
}