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
        Vector3 nextPosition = transform.position + (speed * Time.deltaTime) * direction;

        if(!DoCollisionSweep(nextPosition))
        {
            transform.position = nextPosition;
        }
    }

    private bool DoCollisionSweep(Vector3 nextPosition) 
    {
        float distance = (speed * Time.deltaTime) * 1.5f; // Add a little wiggle room
        Vector3 direction = (nextPosition - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, direction, out hit, distance, ~0)) 
        {
            OnHitSomething(hit.transform.gameObject);
            return true;
        }

        return false;
    }

    private void OnHitSomething(GameObject other)
    {
        Health otherHealth = other.GetComponent<Health>();

        Debug.Log("Collided");

        if(otherHealth)
        {
            bool killed = otherHealth.TakeDamage(damageAmount);
        }

        Destroy(gameObject);
    }
}
