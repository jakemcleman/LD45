using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    public WeaponWielder[] weapons;

    public float maxSightRange = 100;
    public float maxEngagementRange = 100;

    public float yawRate = 45;
    public float barrelRate = 90;

    public float maxBarrelElevation = 60;
    public float minBarrelDepression = 20;

    public float accuracy = 0.95f;
    public float maxMissAmount = 20;
    public float aimAdjustSpeed = 2.0f;
    public float closeEnoughToShoot = 0.99f;

    public bool autoFire = true;
    public float nonAutoFireRate = 1.0f;
    private float fireTimer;


    private GameObject lastTarget = null;
    private Vector3 aimPoint;

    private void Start()
    {
        fireTimer = 0;
    }

    private void OnDrawGizmos()
    {
        if(aimPoint != transform.position)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(aimPoint, 0.5f);
            Gizmos.DrawLine(transform.position, aimPoint);
        }
    }

    private void Update()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        GameObject target = PickTarget(potentialTargets);
        if(target != null)
        {
            Vector3 realTargetPosition = target.transform.position;

            CharacterController targetController = target.GetComponent<CharacterController>();
            Rigidbody targetRB = target.GetComponent<Rigidbody>();

            float distanceToTarget = Vector3.Distance(transform.position, aimPoint);
            Vector3 leadAim = target.transform.position;


            if(targetController != null)
            {
                float projectileTravelTime = distanceToTarget / weapons[0].CurrentWeapon.GetProjectileSpeed();
                leadAim += (targetController.velocity * projectileTravelTime);
            }
            else if(targetRB != null)
            {
                float projectileTravelTime = distanceToTarget / weapons[0].CurrentWeapon.GetProjectileSpeed();
                leadAim += (targetRB.velocity * projectileTravelTime);
            }

            aimPoint = leadAim;
            lastTarget = target;

            if(fireTimer < nonAutoFireRate) fireTimer += Time.deltaTime;

            RotateBaseTowardsTarget(aimPoint);
            RotateArmsToTarget(aimPoint);

            if(IsAimingAtTarget(aimPoint) 
                && distanceToTarget < maxEngagementRange)
            {
                FireWeapons();
            }
        }
        else
        {
            aimPoint = transform.position;
        }
    }
    
    private void FireWeapons()
    {
        if(autoFire)
        {
            foreach(WeaponWielder weapon in weapons)
            {
                weapon.FirePrimary(true);
            }
        }
        else
        {
            for(int i = 0; i < weapons.Length; ++i)
            {
                if(fireTimer > nonAutoFireRate / weapons.Length)
                {
                    weapons[i].FirePrimary(false);
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
            if(Vector3.Dot(weapon.transform.forward, towardsTarget) > closeEnoughToShoot) 
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
            if(Physics.Raycast(transform.position, directionTowards, out hit, dist, ~(1 << 8)))
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
