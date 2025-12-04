using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JetController : MonoBehaviour {
    
    [SerializeField] private Transform camTarget, displayParent;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float maxThrust = 200f, responsiveness = 10f, lift = 135f, throttleIncrement = 0.1f;

    [Header("Jet Texts")]
    [SerializeField] private TextMeshProUGUI playerNametext;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Boosting Mechanisms")]
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float boostDuration = 2f;
    private bool isBoosting = false;

    [Header("Jet Particle Systems")]
    [SerializeField] private ParticleSystem Hyperdrive;
    [SerializeField] private ParticleSystem Shield;
    [SerializeField] private ParticleSystem Heal;
    [SerializeField] private ParticleSystem DamageSmoke;
    [SerializeField] private ParticleSystem Blast;

    [Header("Action Buttons")]
    [SerializeField] private Button readyButton;

    public Button turretButton;
    public Button missileButton;
    public Button cameraButton;
    public Button abilityButton;
    public Button boostButton;

    private Slider ThrottleSlider;

    public bool IsReady;
    public float throttle = 0f;
    public float sensitivity = 2.5f, rotationSensitvityFactor = 15;

    private float xRotation = 0f;
    private float yRotation = 0f;

    public float responseModifier
    {
        get
        {
            return rb.mass / 5f * responsiveness;
        }
    }

    private void HandleJetForces()
    {
        float currentThrust = maxThrust;
        if (isBoosting)
            currentThrust *= boostMultiplier;
        rb.AddForce(currentThrust * throttle * transform.forward);
        // rb.AddTorque(responseModifier * yaw * transform.up);
        // rb.AddTorque(pitch * responseModifier * transform.right);
        // rb.AddTorque(responseModifier * roll * -transform.forward);
        // lift force
        rb.AddForce(lift * throttle * Vector3.up);
    }

}