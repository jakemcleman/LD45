using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlyingNavAvent))]
public class FlyingEnemy : TurretEnemy
{    
    private GameObject lastTarget = null;
    public float targetOrbitRadius = 15.0f;
    public float aboveAmount = 10.0f;

    public float orbitRate = 0.01f;

    private FlyingNavAvent agent;

    private void Start()
    {
        agent = GetComponent<FlyingNavAvent>();
    }

    private void Update()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        GameObject target = PickTarget(potentialTargets);

        if (target != null)
        {
            Vector3 orbitPos = new Vector3(
                targetOrbitRadius * Mathf.Sin(Time.time * orbitRate), 
                aboveAmount, 
                targetOrbitRadius * Mathf.Cos(Time.time * orbitRate));

            agent.targetPosition = target.transform.position + orbitPos;
        }

        AimAndShoot(target);
    }
}
