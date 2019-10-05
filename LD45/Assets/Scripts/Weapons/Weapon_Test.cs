using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Test : MonoBehaviour, IWeapon
{
    public string GetDisplayName()
    {
        return "Test weapon";
    }

    public GameObject[] PrimaryFire(WeaponWielder firerer, bool tryAuto)
    {
        if (!tryAuto)
        {
            Debug.Log("Bang!");
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject[] spawned = new GameObject[1];
            spawned[0] = ball;
            Destroy(ball, 4);
            return spawned;
        }
        else
        {
            return new GameObject[0];
        }
    }
}
