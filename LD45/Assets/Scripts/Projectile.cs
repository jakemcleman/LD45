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
        AudioDefs.Surface surface_type;

        // Determine surface type of hit object
        switch (other.tag)
        {
            case "Wood":
                surface_type = AudioDefs.Surface.Wood;
                break;
            case "Stone":
                surface_type = AudioDefs.Surface.Stone;
                break;
            case "Dirt":
                surface_type = AudioDefs.Surface.Dirt;
                break;
            case "Metal":
                surface_type = AudioDefs.Surface.Metal;
                break;
            case "Sandbag":
                surface_type = AudioDefs.Surface.Sandbag;
                break;
            default:
                surface_type = AudioDefs.Surface.None;
                break;
        }

        Debug.Log(other.tag.ToString());
        
        // Only bother FMOD if there's an impact to play
        if (surface_type != AudioDefs.Surface.None)
        {
            FMOD.Studio.EventInstance ImpactEvent = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Weapons/Bullet_Impact");
            FMOD.ATTRIBUTES_3D att = FMODUnity.RuntimeUtils.To3DAttributes(gameObject);
            ImpactEvent.set3DAttributes(att);
            
            // Set the surface then play the event
            ImpactEvent.setParameterByName("Surface", (int)surface_type);
            ImpactEvent.start();
            ImpactEvent.release();
        }

        Health otherHealth = other.GetComponent<Health>();

        Debug.Log("Collided");

        if(otherHealth)
        {
            bool killed = otherHealth.TakeDamage(damageAmount);

            if(killed)
            {
                // Death audio event here
                Debug.Log("Target destroyed");
            }
            else 
                Debug.Log("Did damage");
        }

        Destroy(gameObject);
    }
}
