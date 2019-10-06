using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWhiz : MonoBehaviour
{
    private string event_path = "event:/SFX/Weapons/Bullet_Passby";

    // Start is called before the first frame update
    void Start()
    {

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

        FMODUnity.RuntimeManager.PlayOneShot(event_path, other.gameObject.transform.position);
    }
}
