using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathPoofTrigger : PoofTrigger
{
    private bool alreadyDied;

    private void Start()
    {
        Health healthComp = GetComponent<Health>();

        healthComp.onDeath.AddListener(OnDeath);

        alreadyDied = false;
    }

    private void OnDeath() 
    {
        // BAD KLUDGE FIX BECAUSE IDK WHY THINGS ARE DOUBLE DYING
        if(alreadyDied) return;

        DoPoof();
    }
}
