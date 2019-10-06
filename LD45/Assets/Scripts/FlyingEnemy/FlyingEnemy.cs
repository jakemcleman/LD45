using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlyingNavAvent))]
public class FlyingEnemy: MonoBehaviour
{    
    public float maxSightRange = 500;
    private GameObject target = null;
    private GameObject lastTarget = null;
    public float targetOrbitRadius = 15.0f;
    public float aboveAmount = 10.0f;

    public float yawRate = 45;

    public float orbitRate = 0.01f;

    private FlyingNavAvent agent;

    private void Start()
    {
        agent = GetComponent<FlyingNavAvent>();
    }

    private void Update()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        target = PickTarget(potentialTargets);

        if (target != null)
        {
            Vector3 orbitPos = new Vector3(
                targetOrbitRadius * Mathf.Sin(Time.time * orbitRate), 
                aboveAmount, 
                targetOrbitRadius * Mathf.Cos(Time.time * orbitRate));

            agent.targetPosition = target.transform.position + orbitPos;
        }

        float curSpeed = agent.Velocity.magnitude;

        if (curSpeed > 0.5f)
        {
            Vector3 horizOnlyVel = agent.Velocity;
            horizOnlyVel.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(horizOnlyVel.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, yawRate * Time.deltaTime);
        }
    }

    private bool CanSee(GameObject target) 
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if(dist <= maxSightRange) 
        {
            RaycastHit hit;
            Vector3 directionTowards = target.transform.position - transform.position;
            if(Physics.Raycast(transform.position, directionTowards, out hit, dist, ~(1 << 8), QueryTriggerInteraction.Ignore))
            {
                if(hit.transform.gameObject == target)
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected GameObject PickTarget(GameObject[] Targets) 
    {
        float bestDist = maxSightRange;
        GameObject best = null;

        foreach(GameObject target in Targets)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if(target == lastTarget) dist /= 2; // Favour last aquired target

            if(dist <= bestDist) 
            {
                if(CanSee(target))
                {
                    best = target;
                    bestDist = dist;
                }
            }
        }
        return best;
    }
    #region FlyingEnemyStates
    void ChangeFlyingEnemyState(FlyingEnemyState nextState)
    {
        if (!_transitionBlocked)
        {
            if (_curState != nextState)
            {
                FlyingEnemyStateEvent flyingEnemyStateEvent;
                flyingEnemyStateEvent.prevState = _curState;
                flyingEnemyStateEvent.nextState = nextState;
                FlyingEnemyStateInitialize(flyingEnemlyStateEvent);
            }
        }
    }
    void FlyingEnemyStateInitialize(FlyingEnemyStateEvent flyingEnemyStatEvent) {
        // Leaving
        switch(flyingEnemyStateEvent.prevState)
        {
            case FlyingEnemyState.Idle:
            {
                
            }
            case FlyingEnemyState.Attacking:
            {
                
            }
            case FlyingEnemyState.Retreating:
            {
                
            }
            default:
            {
                break;
            }
        }
        switch(flyingEnemyStateEvent.nextState)
        {
            case FlyingEnemyState.Idle:
            {

            }
            case FlyingEnemyState.Attacking:
            {
                agent.retreat = false;
                break;
            }
            case FlyingEnemyState.Retreating:
            {
                agent.retreat = true;
                break;
            }
            default: 
            {
                break;
            }
        }
    }
    #endregion
}

