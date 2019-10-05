using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Basic : MonoBehaviour, IWeapon
{
    public GameObject projectilePrefab;

    public int maxAmmo = 12;
    public int curAmmo;

    private void Awake() 
    {
        curAmmo = maxAmmo;
    }

    public string GetDisplayName()
    {
        return "Basic Bitch";
    }

    public string GetInternalName()
    {
        return "Test_Weapon";
    }

    public float GetCurrentAmmoRatio()
    {
        return (float)curAmmo / maxAmmo;
    }

    public float GetCurrentAmmo()
    {
        return curAmmo;
    }

    public float GetMaxAmmo()
    {
        return maxAmmo;
    }

    public bool PrimaryFire(WeaponWielder firer, bool tryAuto)
    {
        if (!tryAuto)
        {
            if(curAmmo == 0) 
            {
                Reload(firer);
                return false;
            }

            GameObject projectile = GameObject.Instantiate(projectilePrefab);
            Projectile proj = projectile.GetComponent<Projectile>();
            if(proj == null) Debug.LogError("Tried to shoot not a projectile");

            projectile.transform.position = transform.position + transform.forward;
            proj.direction = transform.forward;

            curAmmo--;
            
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Reload(WeaponWielder firer)
    {
        if(curAmmo == maxAmmo) return false;

        Debug.Log("Reloading...");
        curAmmo = maxAmmo;
        return true;
    }
}
