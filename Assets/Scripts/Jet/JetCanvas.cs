using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JetCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gforceIndicator;
    public Vector3 GForce;
    private void Update()
    {
        //gforceIndicator.text = string.Format("{0:0.0} G", GForce);

        float verticalG = GForce.y;
        gforceIndicator.text = verticalG.ToString("F1") + " G";
    }
}