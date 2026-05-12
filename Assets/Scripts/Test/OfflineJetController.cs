using System;
using System.Globalization;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OfflineJetController : MonoBehaviour
{
    public Rigidbody jetRb;

    [SerializeField] private InputActionReference lookAction;

    public GameObject TouchPad;
    float mouseX, mouseY;

    public Slider throttleSlider;

    bool rollLeftPressed = false;
    bool rollRightPressed = false;

    [Header("Physics")]
    [SerializeField] float rollTorque = 20f;
    [SerializeField] float rollStabilize = 5f;
    [SerializeField] float speedMult = 1f;
    [SerializeField] float speedMultAngle = 0.5f;

    [Header("Boost")]
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float boostDuration = 2f;

    private bool isBoosting = false;
    private float boostTimer = 0f;

    //private JetCanvas jetCanvas;
    //private JetStats jetStats;

    Vector3 lastVelocity;
    public Vector3 LocalGForce;

    [SerializeField] private TextMeshProUGUI healthIndicator;
    [SerializeField] private TextMeshProUGUI gforceIndicator;
    [SerializeField] private TextMeshProUGUI speedIndicator;
    [SerializeField] private TextMeshProUGUI altitudeIndicator;
    [SerializeField] private TextMeshProUGUI bankAngleIndicator;
    [SerializeField] private GameObject hudHorizontalIndicator;

    public Vector3 GForce;
    public Transform cameraTransform;



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

    private float missileCooldownTimer = 0f;
    private bool missileOnCooldown = false;

    private Vector3 originalScale;
    private Color originalColor;

    //public PlayerJet playerJet;

    //protected override void OnSpawned()
    //{
    //    base.OnSpawned();
    //    enabled = isOwner;

    //    cam.gameObject.SetActive(isOwner);

    //    if (UIManager.Instance != null)
    //        UIManager.Instance.OnLocalJetSpawned(hideDelay: 2f);
    //    else
    //        Debug.LogWarning("[PlayerJet] UIManager instance not found in scene.");
    //}

    void OnEnable()

    {
        lookAction.action.Enable();
    }

    void OnDisable()
    {
        lookAction.action.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        jetRb = GetComponent<Rigidbody>();
        //jetCanvas = GetComponent<JetCanvas>();
        //jetStats = GetComponent<JetStats>();
        //cam = Camera.main;
        //jetCanvas.cameraTransform = cam.transform;

        originalScale = buttonTransform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {
        Vector2 input = lookAction.action.ReadValue<Vector2>();
        mouseX = input.x;
        mouseY = input.y;

        Vector3 gForceInGs = LocalGForce / 9.81f;
        GForce = gForceInGs;
        //gforceIndicator.text = string.Format("{0:0.0} G", GForce);

        float verticalG = Math.Abs(GForce.y * 2.5f);
        gforceIndicator.text = verticalG.ToString("F1") + " G";

        float speedMS = jetRb.linearVelocity.magnitude;
        speedIndicator.text = speedMS.ToString("F1") + " m/s";
        altitudeIndicator.text = transform.position.y.ToString("F1");

        float bankAngle =transform.eulerAngles.z;
        if (bankAngle > 180f) bankAngle -= 360f; // remap 180–360 → 0 to -180
        bankAngleIndicator.text = bankAngle.ToString("F1") + "°";


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

    void LateUpdate()
    {
        float parentZ = transform.eulerAngles.z;
        hudHorizontalIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, -parentZ);
        //Vector3 forward = cameraTransform.forward;
        //forward.z = 0f; // Remove roll influence

        //if (forward != Vector3.zero)
        //    hudHorizontalIndicator.transform.rotation = Quaternion.LookRotation(forward);
    }

    public void SetHealth(int health)
    {
        healthIndicator.text = health.ToString();
    }

    private void FixedUpdate()
    {
        float currentThrust = throttleSlider.value * speedMult * (isBoosting ? boostMultiplier : 1f);
        jetRb.AddForce(jetRb.transform.TransformDirection(Vector3.forward) * currentThrust, ForceMode.VelocityChange);
        //jetRb.AddForce(lift * throttleSlider.value * Vector3.up);
        //jetRb.AddForce(jetRb.transform.TransformDirection(Vector3.right) * mouseX * speedMult, ForceMode.Impulse);
        jetRb.AddTorque(jetRb.transform.right * speedMultAngle * mouseY * -1, ForceMode.Acceleration);
        jetRb.AddTorque(jetRb.transform.up * speedMultAngle * mouseX, ForceMode.Acceleration);
        jetRb.AddTorque(jetRb.transform.forward * speedMultAngle * mouseX * -1, ForceMode.Acceleration);
        HandleRoll();
        HandleBoost();
        CalculateGForce(Time.fixedDeltaTime);
    }

    void HandleRoll()
    {
        if (rollLeftPressed)
        {
            jetRb.AddTorque(transform.forward * rollTorque, ForceMode.Acceleration);
        }
        else if (rollRightPressed)
        {
            jetRb.AddTorque(-transform.forward * rollTorque, ForceMode.Acceleration);
        }
        else
        {
            float rollVelocity = jetRb.angularVelocity.z;
            float stabilizeTorque = -rollVelocity * rollStabilize;

            jetRb.AddTorque(Vector3.forward * stabilizeTorque, ForceMode.Acceleration);
        }
    }

    public void OnRollLeftDown()
    {
        rollLeftPressed = true;
    }

    public void OnRollLeftUp()
    {
        rollLeftPressed = false;
    }

    public void OnRollRightDown()
    {
        rollRightPressed = true;
    }

    public void OnRollRightUp()
    {
        rollRightPressed = false;
    }

    void CalculateGForce(float dt)
    {
        // Current velocity from Rigidbody
        Vector3 velocity = jetRb.linearVelocity;
        // Acceleration = change in velocity over time
        Vector3 acceleration = (velocity - lastVelocity) / dt;
        // Remove gravity (so you only measure maneuver Gs)
        acceleration -= Physics.gravity;
        // Convert to local space (relative to jet orientation)
        LocalGForce = transform.InverseTransformDirection(acceleration);
        // Store for next frame
        lastVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("JET COLLIDED WITH " + collision.gameObject.name);
    }

    public void OnBoostPressed()
    {
        if (isBoosting) return; // prevent re-triggering mid-boost

        isBoosting = true;
        boostTimer = boostDuration;
    }

    private void HandleBoost()
    {
        if (!isBoosting) return;

        boostTimer -= Time.fixedDeltaTime;
        if (boostTimer <= 0f)
        {
            isBoosting = false;
            boostTimer = 0f;
        }
    }


    public void OnFireDown()
    {
        isFirePressed = true;
        buttonImage.color = Color.green;
        buttonTransform.localScale = originalScale * 0.9f;

        // Fire immediately, then repeat every fireRate seconds
        Fire();
        InvokeRepeating(nameof(Fire), fireRate, fireRate);
    }

    public void OnFireUp()
    {
        isFirePressed = false;
        buttonImage.color = originalColor;
        buttonTransform.localScale = originalScale;

        // Stop the repeating fire
        CancelInvoke(nameof(Fire));
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
        if ( missileOnCooldown) return;

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

        Missile missile = missileObj.GetComponent<Missile>();
        missile.Initialize(aimPoint);
    }
}
