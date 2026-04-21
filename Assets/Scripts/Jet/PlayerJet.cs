using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJet : MonoBehaviour
{
    Rigidbody jetRb;
    InputActionAsset InputActions;

    public InputActionReference lookAction;

    public GameObject TouchPad;
    float verticalMove, horizontalMove, mouseX, mouseY, rollInput;

    public Slider throttleSlider;

    bool rollLeftPressed = false;
    bool rollRightPressed = false;

    [SerializeField] float rollTorque = 20f;
    [SerializeField] float rollStabilize = 5f;

    [SerializeField] float speedMult = 1f;
    [SerializeField] float speedMultAngle = 0.5f ;
    [SerializeField] float speedRollMultAngle = 0.05f;
    [SerializeField] float torqueStrength = 10f;

    [SerializeField] float maxRollAngle = 90f;
    [SerializeField] float rollTorqueStrength = 10f;
    [SerializeField] float rollDamping = 3f;

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

        //look = InputActions.FindAction("Look");
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 input = lookAction.action.ReadValue<Vector2>();

        mouseX = input.x;
        mouseY = input.y;

        //verticalMove = Input.GetAxis("Vertical");
        //Debug.Log("MOBILE ACTIONS : " + look.verti)
    }


    private void FixedUpdate()
    {
        jetRb.AddForce(jetRb.transform.TransformDirection(Vector3.forward) * throttleSlider.value * speedMult, ForceMode.VelocityChange );
        //jetRb.AddForce(jetRb.transform.TransformDirection(Vector3.right) * mouseX * speedMult, ForceMode.Impulse);
        jetRb.AddTorque(jetRb.transform.right * speedMultAngle * mouseY * -1, ForceMode.Acceleration);
        jetRb.AddTorque(jetRb.transform.up * speedMultAngle * mouseX , ForceMode.Acceleration);
        jetRb.AddTorque(jetRb.transform.forward * speedMultAngle * mouseX * -1, ForceMode.Acceleration);

        HandleRoll();
        //StabilizeRoll();
        //float zAngle = transform.eulerAngles.z;

        //if (zAngle > 180f)
        //    zAngle -= 360f;

        //float correction = -zAngle * torqueStrength;

        //jetRb.AddTorque(Vector3.forward * correction);
    }

    void StabilizeRoll()
    {
        // Get current roll angular velocity
        float rollVelocity = jetRb.angularVelocity.z;

        // Apply damping torque to stop roll
        float torque = -rollVelocity * torqueStrength;

        jetRb.AddTorque(Vector3.forward * torque, ForceMode.Acceleration);
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
            // Optional: stabilize roll when no input
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

}
