using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    public WeaponWielder[] weapons;

    public float maxSightRange = 100;

    public float yawRate = 45;
    public float barrelRate = 90;

    public float maxBarrelElevation = 60;
    public float minBarrelDepression = 20;

    public float accuracy = 0.95f;

    private GameObject lastTarget = null;

    private void Update()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        GameObject target = PickTarget(potentialTargets);
        lastTarget = target;

        if(target != null) 
        {
            
            RotateBaseTowardsTarget(target.transform.position);
            RotateArmsToTarget(target.transform.position);

            if(IsAimingAtTarget(target.transform.position))
            {
                foreach(WeaponWielder weapon in weapons)
                {
                    weapon.FirePrimary(true);
                }
            }
        }
    }

    private void RotateBaseTowardsTarget(Vector3 targetPos)
    {
        Vector3 towardsTarget = targetPos - transform.position;
        towardsTarget.y = 0;
        
        Quaternion lookRotation = Quaternion.LookRotation(towardsTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, yawRate * Time.deltaTime);
    }

    private void RotateArmsToTarget(Vector3 targetPos)
    {
        Vector3 localTargetPos = transform.InverseTransformPoint(targetPos);
        localTargetPos.x = 0.0f;

        // Clamp target rotation by creating a limited rotation to the target.
        // Use different clamps depending if the target is above or below the turret.
        Vector3 clampedLocalVec2Target = localTargetPos;
        if (localTargetPos.y >= 0.0f)
            clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * maxBarrelElevation, float.MaxValue);
        else
            clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * minBarrelDepression, float.MaxValue);

        Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
        

        foreach(WeaponWielder weapon in weapons)
        {
            Quaternion newRotation = Quaternion.RotateTowards(weapon.transform.localRotation, rotationGoal, 2.0f * barrelRate * Time.deltaTime);
            weapon.transform.localRotation = newRotation;
        }
    }

    private bool IsAimingAtTarget(Vector3 targetPos)
    {
        Vector3 towardsTarget = (targetPos - transform.position).normalized;
    
        foreach(WeaponWielder weapon in weapons)
        {
            if(Vector3.Dot(weapon.transform.forward, towardsTarget) > accuracy) 
            {
                return true;
            }
        }

        return false;
    }

    private bool CanSee(GameObject target) 
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if(dist <= maxSightRange) 
        {
            RaycastHit hit;
            Vector3 directionTowards = target.transform.position - transform.position;
            if(Physics.Raycast(transform.position, directionTowards, out hit, dist))
            {
                if(hit.transform.gameObject == target)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private GameObject PickTarget(GameObject[] Targets) 
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
}
