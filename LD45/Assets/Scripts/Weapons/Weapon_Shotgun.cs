using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Shotgun : MonoBehaviour, IWeapon
{
    public GameObject projectilePrefab;

    public int maxAmmo = 6;
    public int curAmmo;

    public int projectilesPerShot = 12;

    /*
    * Width in degrees of the cone of bullet spread
    */
    public float spreadAngle = 10;

    private void Awake() 
    {
        curAmmo = maxAmmo;
    }

    public string GetDisplayName()
    {
        return "Shotgun";
    }

    public string GetInternalName()
    {
        return "Shotgun";
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

            float spreadLinearMax = Mathf.Sin(Mathf.Deg2Rad * (spreadAngle / 2));

            for(int i = 0; i < projectilesPerShot; ++i)
            {
                float spreadLinear = Random.Range(0, spreadLinearMax);
                Vector3 fireVec = Quaternion.AngleAxis(Random.Range(0, 360), transform.forward) * (transform.forward + (transform.up * spreadLinear));

                GameObject projectile = GameObject.Instantiate(projectilePrefab);
                Projectile proj = projectile.GetComponent<Projectile>();
                if(proj == null) Debug.LogError("Tried to shoot not a projectile");

                projectile.transform.position = transform.position + transform.forward;
                proj.direction = fireVec;
            }

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
