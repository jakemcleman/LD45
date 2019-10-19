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

    public float persistAfterHitTime = 1.0f;

    public bool hasHit;

    private void Start()
    {
        // Projectile should only live for <timeout> seconds
        Destroy(gameObject, Timeout);

        hasHit = false;
    }

    private void Update()
    {
        if(hasHit) return;

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
        if(Physics.Raycast(transform.position, direction, out hit, distance, ~0, QueryTriggerInteraction.Ignore)) 
        {
            OnHitSomething(hit.transform.gameObject, hit.point);
            return true;
        }

        return false;
    }

    private void OnHitSomething(GameObject other, Vector3 hitPoint)
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

        if(otherHealth)
        {
            bool killed = otherHealth.TakeDamage(damageAmount);

            if(killed)
            {
                // Death audio event here
                Debug.Log("Target destroyed");
            }
        }

        speed = 0;
        hasHit = true;
        transform.position = hitPoint;
        GetComponent<MeshRenderer>().enabled = false;
        Destroy(gameObject, persistAfterHitTime);
    }
}
