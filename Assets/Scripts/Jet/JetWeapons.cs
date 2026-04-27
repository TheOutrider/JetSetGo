using UnityEngine;
using PurrNet;
using UnityEngine.UI;
using Unity.Cinemachine;
public class JetWeapons : NetworkBehaviour
{

    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 2000f;

    private bool isFirePressed = false;
    public RectTransform buttonTransform;
    public Image buttonImage;

    [SerializeField] private Transform gunMuzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 1000f;

    private Vector3 originalScale;
    private Color originalColor;
    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;

        originalScale = buttonTransform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {
        if (!isFirePressed) return;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, hitLayer))
            return;

        Debug.Log($"HIT OBJ : {hit.transform.name}");
    }

    public void OnFireDown()
    {
        isFirePressed = true;
        buttonImage.color = Color.green;
        buttonTransform.localScale = originalScale * 0.9f;
        Fire();
    }

    public void OnFireUp()
    {
        isFirePressed = false;
        buttonImage.color = originalColor;
        
        buttonTransform.localScale = originalScale;

       
    }

    Vector3 GetAimPoint()
    {

        //Camera cam = Camera.main;
        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitLayer))
        {
            return hit.point;
        }

        // fallback (if nothing hit)
        return ray.origin + ray.direction * range;
    }

    void Fire()
    {
        if (!isOwner) return;
        Vector3 aimPoint = GetAimPoint();

        Vector3 shootDir = (aimPoint - gunMuzzle.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = shootDir * bulletSpeed;
    }
}
