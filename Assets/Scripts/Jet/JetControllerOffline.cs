using System;
using System.Globalization;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JetControllerOffline : MonoBehaviour
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

    private JetCanvasOffline jetCanvas;
    private JetStatsOffline jetStats;

    Vector3 lastVelocity;
    public Vector3 LocalGForce;

    public Vector3 GForce;
    public Transform cameraTransform;

    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 2000f;

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
        jetCanvas = GetComponent<JetCanvasOffline>();
        jetStats = GetComponent<JetStatsOffline>();
        //cam = Camera.main;
        jetCanvas.cameraTransform = cam.transform;

        jetCanvas.playerJet = this;
    }

    void Update()
    {
        Vector2 input = lookAction.action.ReadValue<Vector2>();
        mouseX = input.x;
        mouseY = input.y;

        Vector3 gForceInGs = LocalGForce / 9.81f;
        GForce = gForceInGs;
        //gforceIndicator.text = string.Format("{0:0.0} G", GForce);
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

    public void ApplyJetData(JetData data)
    {
        rollTorque = data.rollTorque;
        rollStabilize = data.rollStabilize;
        speedMult = data.speedMultiplier;
        speedMultAngle = data.speedMultiplierAngle;

        // Start thruster particles if any are defined
        foreach (var thruster in data.thrusters)
        {
            if (thruster != null) thruster.Play();
        }
    }

}