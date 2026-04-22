using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JetCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gforceIndicator;
    [SerializeField] private TextMeshProUGUI speedIndicator;
    [SerializeField] private TextMeshProUGUI altitudeIndicator;
    [SerializeField] private GameObject hudHorizontalIndicator;
    public Vector3 GForce;
    public Transform cameraTransform;
    public PlayerJet playerJet;

    private void Update()
    {
        //gforceIndicator.text = string.Format("{0:0.0} G", GForce);

        float verticalG = GForce.y;
        gforceIndicator.text = verticalG.ToString("F1") + " G";

        float speedMS = playerJet.jetRb.linearVelocity.magnitude;
        speedIndicator.text = speedMS.ToString("F1") + " m/s";
        altitudeIndicator.text = playerJet.transform.position.y.ToString("F1");
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
}