using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJet : NetworkBehaviour
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

    private JetCanvas jetCanvas;
    private JetStats jetStats;

    Vector3 lastVelocity;
    public Vector3 LocalGForce;

    [SerializeField] private CinemachineCamera cam;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;

        cam.gameObject.SetActive(isOwner);
    }

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
        jetCanvas = GetComponent<JetCanvas>();
        jetStats = GetComponent<JetStats>();
        //cam = Camera.main;
        jetCanvas.playerJet = this;
        jetCanvas.cameraTransform = cam.transform;
    }

    void Update()
    {
        Vector2 input = lookAction.action.ReadValue<Vector2>();
        mouseX = input.x;
        mouseY = input.y;

        Vector3 gForceInGs = LocalGForce / 9.81f;
        jetCanvas.GForce = gForceInGs;
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
}
