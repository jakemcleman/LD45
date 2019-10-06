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
    void Start()
    {
        Health healthComp = GetComponent<Health>();

        healthComp.onDeath.AddListener(OnDeath);
    }

    private void OnDeath() 
    {
        foreach(GameObject toSpawn in spawnOnDeath) 
        {
            GameObject spawned = GameObject.Instantiate(toSpawn);
            spawned.transform.position = transform.position;
            spawned.transform.rotation = transform.rotation;
        }

        Destroy(gameObject);
    }

}
