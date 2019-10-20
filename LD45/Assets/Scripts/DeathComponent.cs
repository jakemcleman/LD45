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

    private bool alreadyDied;

    void Start()
    {
        Health healthComp = GetComponent<Health>();

        healthComp.onDeath.AddListener(OnDeath);

        alreadyDied = false;
    }

    private void OnDeath() 
    {
        // BAD KLUDGE FIX BECAUSE IDK WHY THINGS ARE DOUBLE DYING
        if(alreadyDied) return;

        alreadyDied = true;
        
        // Kill all the children
        foreach(Health childHealth in GetComponentsInChildren<Health>())
        {
            // Apparently I need this because objects count as their own fucking child?
            if(GetComponent<Health>() != childHealth)
            {
                childHealth.TakeDamage(float.MaxValue);
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
        
        Destroy(gameObject);
    }

}
