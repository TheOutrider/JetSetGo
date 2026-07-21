using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
public class JetWeaponsOffline : MonoBehaviour
{

    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 2000f;

    private bool isFirePressed = false;
    public RectTransform buttonTransform;
    public Image buttonImage;

    [Header("Bullet")]
    [SerializeField] private Transform gunMuzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 1000f;
    [SerializeField] private float fireRate = 0.1f;

    [Header("Missile")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform missileMuzzle;
    [SerializeField] private float missileCooldown = 3f;
    public Button missileButton;    

    public int bulletCount, missileCount;              

    private float missileCooldownTimer = 0f;
    private bool missileOnCooldown = false;

    private Vector3 originalScale;
    private Color originalColor;

    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Audio")]
    [SerializeField] private AudioSource machineGunAudioSource;


    void Start()
    {
         originalScale = buttonTransform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {

        if (missileOnCooldown)
        {
            missileCooldownTimer -= Time.deltaTime;
            if (missileCooldownTimer <= 0f)
            {
                missileCooldownTimer = 0f;
                missileOnCooldown = false;
                missileButton.interactable = true;
            }
        }

        if (!isFirePressed) return;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, hitLayer))
            return;

        Debug.Log($"HIT OBJ : {hit.transform.name}");
    }

    public void OnFireDown()
    {
        if(bulletCount > 0) return;
        isFirePressed = true;
        buttonImage.color = Color.green;
        buttonTransform.localScale = originalScale * 0.9f;

        Fire();
        muzzleFlash.Play();
        InvokeRepeating(nameof(Fire), fireRate, fireRate);
        InvokeRepeating(nameof(PlayMuzzleFlash), fireRate, fireRate); 

        machineGunAudioSource.Play();
    }

    public void OnFireUp()
    {
        isFirePressed = false;
        buttonImage.color = originalColor;
        buttonTransform.localScale = originalScale;

        CancelInvoke(nameof(Fire));
        CancelInvoke(nameof(PlayMuzzleFlash));
        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); 

        machineGunAudioSource.Stop  ();
    }

    void PlayMuzzleFlash()
    {
        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        muzzleFlash.Play();
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
        Vector3 aimPoint = GetAimPoint();
        Vector3 shootDir = (aimPoint - gunMuzzle.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = shootDir * bulletSpeed;
    }

    public void OnMissilePressed()
    {
        if(missileCount > 0) return;
        if (missileOnCooldown) return;

        FireMissile();

        // Start cooldown
        missileOnCooldown = true;
        missileCooldownTimer = missileCooldown;
        missileButton.interactable = false;
    }

    void FireMissile()
    {
        Vector3 aimPoint = GetAimPoint();
        Vector3 shootDir = (aimPoint - missileMuzzle.position).normalized;

        GameObject missileObj = Instantiate(
            missilePrefab,
            missileMuzzle.position,
            Quaternion.LookRotation(shootDir)
        );

        MissileOffline missile = missileObj.GetComponent<MissileOffline>();
        missile.Initialize(aimPoint);
    }
    
}


