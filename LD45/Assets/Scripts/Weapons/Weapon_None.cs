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

    public string GetInternalName()
    {
        return "None";
    }

    public float GetCurrentAmmoRatio()
    {
        return 0.0f;
    }

    public bool PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        return false;
    }

    public bool Reload(WeaponWielder firerer)
    {
        return false;
    }

    public float GetCurrentAmmo()
    {
        return 0;
    }

    public float GetMaxAmmo()
    {
        return 0;
    }
}
