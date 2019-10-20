using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathComponent : MonoBehaviour
{
    /*
     *  Set of game objects that should all be spawned when this entity dies
     */
    public GameObject[] spawnOnDeath;

    [FMODUnity.EventRef]
    public string death_event;

    void Start()
    {
        Health healthComp = GetComponent<Health>();

        healthComp.onDeath.AddListener(OnDeath);
    }

    private void OnDeath() 
    {
        // Kill all the children
        foreach(Health childHealth in GetComponentsInChildren<Health>())
        {
            // Apparently I need this because objects count as their own fucking child?
            if(GetComponent<Health>() != childHealth)
            {
                childHealth.Kill();
            }
        }       

        // Play death sound
        FMODUnity.RuntimeManager.PlayOneShot(death_event, transform.position);

        foreach(GameObject toSpawn in spawnOnDeath) 
        {
            GameObject spawned = GameObject.Instantiate(toSpawn);
            spawned.transform.position = transform.position;
            spawned.transform.rotation = transform.rotation;
        }
        
        CheckpointReset cr = GetComponent<CheckpointReset>();
        if (cr == null)
        {
            Destroy(gameObject);
        }
        else
        {
            cr.SetDead();
        }
    }

}
