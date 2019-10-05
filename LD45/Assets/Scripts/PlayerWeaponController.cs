using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponWielder))]
public class PlayerWeaponController : MonoBehaviour
{
    public GameObject[] weapons;

    private WeaponWielder wielder;

    private void Start()
    {
        wielder = GetComponent<WeaponWielder>();

        wielder.CurrentWeapon = weapons[0].GetComponent<IWeapon>();
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
    }
}
