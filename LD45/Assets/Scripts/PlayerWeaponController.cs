using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponWielder))]
public class PlayerWeaponController : MonoBehaviour
{
    public GameObject[] weapons;

    private int curWeaponIndex;
    private WeaponWielder wielder;

    private void Start()
    {
        wielder = GetComponent<WeaponWielder>();

        curWeaponIndex = 0;
        wielder.CurrentWeapon = weapons[curWeaponIndex].GetComponent<IWeapon>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            wielder.FirePrimary(false);
        }
        else if (Input.GetButton("Fire1"))
        {
            wielder.FirePrimary(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            wielder.Reload();
        }

        // Test weapon switching code - if this is a real thing we want do this better with a real input button
        if (Input.GetKeyDown(KeyCode.E))
        {
            curWeaponIndex++;
            if (curWeaponIndex >= weapons.Length) curWeaponIndex = 0;

            wielder.CurrentWeapon = weapons[curWeaponIndex].GetComponent<IWeapon>();
        }
    }
}
