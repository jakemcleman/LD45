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

    private void Update()
    {
        transform.forward = direction;
        transform.position += (speed * Time.deltaTime) * direction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Health otherHealth = collision.gameObject.GetComponent<Health>();

        if(otherHealth)
        {
            otherHealth.TakeDamage(damageAmount);
        }

        Destroy(gameObject);
    }
}
