using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Test : MonoBehaviour, IWeapon
{
    public string GetDisplayName()
    {
        return "Test weapon";
    }

    public string GetInternalName()
    {
        return "Test_Weapon";
    }

    public float GetCurrentAmmoRatio()
    {
        return 1.0f;
    }

    public bool Reload(WeaponWielder firerer)
    {
        return false;
    }

    public bool PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        if (!tryAuto)
        {
            Debug.Log("Bang!");
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.position = transform.position;
            Destroy(ball, 4);
            return true;
        }
        else
        {
            return false;
        }
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
