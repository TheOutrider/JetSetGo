using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DroneUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar, healthBarFP, speedometer, speedometerFP;
    
    private GameObject drone;
    private DroneController droneController;

    [SerializeField]
    private GameObject FPC, TPC, FPCanvas, TPCanvas;

    private bool cameraSwap = false, hasLauncherAcquired = false;

    private void Start()
    {
        drone = GameObject.FindWithTag("Player");
        if (drone != null)
        {
            droneController = drone.gameObject.GetComponent<DroneController>();
        }
    }

    private void Update()
    {
        HandleCameraPOV();
        UpdateHealthBar(droneController.health, 200f);
        UpdateSpeedometer(droneController.throttle, droneController.maxThrottle);
    }

    private void UpdateHealthBar(float currentValue, float maxValue)
    {
        healthBar.value = currentValue / maxValue;
        healthBarFP.value = currentValue / maxValue;
    }

    private void UpdateSpeedometer(float currentValue, float maxValue)
    {
        speedometer.value = currentValue / maxValue;
        speedometerFP.value = currentValue / maxValue;
    }

    private void HandleCameraPOV()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraSwap = !cameraSwap;
            FPC.SetActive(cameraSwap);
            FPCanvas.SetActive(cameraSwap);
            TPC.SetActive(!cameraSwap);
            TPCanvas.SetActive(!cameraSwap);
        }
    }




}