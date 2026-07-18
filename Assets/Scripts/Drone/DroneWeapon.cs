using UnityEngine;
using TMPro;
using System;

public class DroneWeapon : MonoBehaviour
{
    public int gunAmmo = 100, fireballAmmo = 0;

    [SerializeField] private TextMeshProUGUI gunAmmoTextFP, fireballAmmoFP, gunAmmoTextTP, fireballAmmoTP;
    [SerializeField] private GameObject gunAmmoUI, fireballAmmoUI, gunAmmoUIFP, fireballAmmoUIFP, launchers;

    [Header("Drone Audios")]
    [SerializeField] private AudioSource droneAudioSource;
    [SerializeField] private AudioClip turret, emptyGun, flame;

    [SerializeField] private GameObject[] turrets;
    [Header("Prefabs & Particles")]
    [SerializeField] private GameObject bulletPrefab, fireballPrefab;

    public delegate void OnRoundsFired(int ammo);
    public static event OnRoundsFired RoundsFired;


    private bool isGunSelected = true;

    private void Awake()
    {

    }

    private void Start()
    {
        DroneController.OnGunFired += GunFired;
        DroneController.OnGunAmmoAcquired += OnGunAmmoAcquired;
        DroneController.OnFireballAmmoAcquired += OnFireballAmmoAcquired;
        DroneController.WeaponChanged += OnWeaponChanged;
    }

    private void OnWeaponChanged(bool selectedGun)
    {
        isGunSelected = selectedGun;
        launchers.SetActive(!isGunSelected);
        gunAmmoUI.gameObject.SetActive(selectedGun);
        fireballAmmoUI.gameObject.SetActive(!selectedGun);
        gunAmmoUIFP.gameObject.SetActive(selectedGun);
        fireballAmmoUIFP.gameObject.SetActive(!selectedGun);
    }

    private void GunFired()
    {
        if ((gunAmmo > 0 && isGunSelected) || (fireballAmmo > 0 && !isGunSelected))
        {
            droneAudioSource.clip = isGunSelected ? turret : flame;
            droneAudioSource.Play();
            if (isGunSelected)
            {
                gunAmmo -= 1;
            }
            else
            {
                if (!isGunSelected)
                    fireballAmmo -= 1;
            }

            foreach (var turret in turrets)
            {
                if (isGunSelected && gunAmmo > 0)
                {
                    GameObject bullet = Instantiate(bulletPrefab, turret.transform.position, turret.transform.rotation);
                }
                else if (!isGunSelected && fireballAmmo > 0)
                {
                    GameObject bullet = Instantiate(fireballPrefab, turret.transform.position, turret.transform.rotation);
                }

            }

            RoundsFired?.Invoke(isGunSelected ? gunAmmo : fireballAmmo);
        }
        else
        {
            droneAudioSource.clip = emptyGun;
            droneAudioSource.Play();
        }
    }

    private void OnGunAmmoAcquired()
    {
        gunAmmo += 100;

    }

    private void OnFireballAmmoAcquired()
    {
        fireballAmmo += 5;

    }

    private void Update()
    {
        gunAmmoTextFP.text = "x " + gunAmmo.ToString();
        gunAmmoTextTP.text = "x " + gunAmmo.ToString();
        fireballAmmoFP.text = "x " + fireballAmmo.ToString();
        fireballAmmoTP.text = "x " + fireballAmmo.ToString();
    }




}