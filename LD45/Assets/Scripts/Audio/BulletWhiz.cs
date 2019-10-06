using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWhiz : MonoBehaviour
{
    FMOD.Studio.EventInstance BulletWhiz_Event;
    FMOD.ATTRIBUTES_3D att;

    // Start is called before the first frame update
    void Start()
    {
        BulletWhiz_Event = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Weapons/Bullet_Passby");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Projectile")
            return;

        Debug.Log("BULLET PASSED BY PLAYER!");

        att = FMODUnity.RuntimeUtils.To3DAttributes(other.gameObject);
        BulletWhiz_Event.set3DAttributes(att);
        BulletWhiz_Event.start();
    }
}
