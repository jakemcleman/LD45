using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // The weapon that launched this projectile 
    // (so it can be informed about what happened to it if required)
    public IWeapon shooter;

    // How much hurt
    public float damageAmount;

    // How fast go
    public float speed;

    // Normalized direction this projectile should move in
    public Vector3 direction;
    
    public float Timeout = 30;

    private void Start()
    {
        // Projectile should only live for <timeout> seconds
        Destroy(gameObject, Timeout);
    }

    private void Update()
    {
        transform.forward = direction;
        transform.position += (speed * Time.deltaTime) * direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        Health otherHealth = other.gameObject.GetComponent<Health>();

        Debug.Log("Collided");

        if(otherHealth)
        {
            bool killed = otherHealth.TakeDamage(damageAmount);
            if(killed) Debug.Log("Target destroyed");
            else Debug.Log("Did damage");
        }

        Destroy(gameObject);
    }
}
