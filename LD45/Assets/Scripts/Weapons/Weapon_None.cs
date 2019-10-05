using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Dummy 'weapon' that does nothing
 */
public class Weapon_None : MonoBehaviour, IWeapon
{
    public string GetDisplayName()
    {
        return "None";
    }

    public GameObject[] PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        return new GameObject[0];
    }
}
