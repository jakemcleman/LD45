using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Basic : MonoBehaviour, IWeapon
{
    public GameObject projectilePrefab;

    public string GetDisplayName()
    {
        return "Basic Bitch";
    }

    public string GetInternalName()
    {
        return "BasicGun";
    }

    public float GetCurrentAmmoRatio()
    {
        return 0.0f;
    }

    public bool PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        if (!tryAuto)
        {
            GameObject projectile = GameObject.Instantiate(projectilePrefab);
            Projectile proj = projectile.GetComponent<Projectile>();
            if(proj == null) Debug.LogError("Tried to shoot not a projectile");

            projectile.transform.position = transform.position + transform.forward;
            proj.direction = transform.forward;
            
            return true;
        }
        else
        {
            return false;
        }
    }
}
