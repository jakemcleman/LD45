using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private Transform displayMesh;

    public float spinRate = 90.0f;
    public float bobMagnitide = 1.0f;
    public float bobRate = 2.0f;

    public int weaponIndex = 1;

    private static string[] weaponNames = new string[] {"None", "Pistol", "Shotgun", "SMG", "Railgun"};

    private void Start()
    {
        displayMesh = transform.Find("WeapMesh");
    }

    private void Update()
    {
        displayMesh.localPosition = bobMagnitide * 0.5f * Mathf.Sin(Time.time * bobRate) * Vector3.up;
        displayMesh.Rotate(new Vector3(0, spinRate * Time.deltaTime, 0));
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            PlayerWeaponController wepCon = col.GetComponent<PlayerWeaponController>();
            if (wepCon != null){
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Pickup_Weapon", transform.position);
                wepCon.UnlockWeapon(weaponIndex);
                Destroy(gameObject);
            }

            AnalyticsReporter.ReportPickup(transform.position, weaponNames[weaponIndex] + "_pickup");
        }
    }
}
