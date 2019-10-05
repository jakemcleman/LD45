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
        return 0.0f;
    }

    public bool PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        if (!tryAuto)
        {
            Debug.Log("Bang!");
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject[] spawned = new GameObject[1];
            spawned[0] = ball;
            ball.transform.position = transform.position;
            Destroy(ball, 4);
            return true;
        }
        else
        {
            return false;
        }
    }
}
