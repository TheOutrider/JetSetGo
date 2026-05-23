using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "JetDatabase",
    menuName = "Jet Game/Jet Database")]
public class JetDatabase : ScriptableObject
{
    public List<JetData> jets = new List<JetData>();
}

[System.Serializable]
public class JetData
{
    public string jetName;

    [Header("Visual")]
    public GameObject jetBody;

    [Header("Flight Settings")]
    public float rollTorque = 10f;
    public float rollStabilize = 5f;

    [Header("Speed")]
    public float speedMultiplier = 1f;
    public float speedMultiplierAngle = 15f;

    [Header("Stats")]
    public float maxHealth = 100f;

}