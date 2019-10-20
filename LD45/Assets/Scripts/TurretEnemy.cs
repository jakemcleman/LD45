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

    public bool limitYaw = false;
    public float leftYawLimit = 90;
    public float rightYawLimit = 90;

    public float accuracy = 0.95f;
    public float maxMissAmount = 20;
    public float aimAdjustSpeed = 2.0f;
    public float closeEnoughToShoot = 0.99f;

    public bool autoFire = true;
    public float nonAutoFireRate = 1.0f;
    private float fireTimer;

    private Health health;

    private GameObject lastTarget = null;
    private Vector3 aimPoint;
    private Vector3 idleAimPoint;

    private void Start()
    {
        fireTimer = 0;

        idleAimPoint = transform.position + 10 * transform.forward;

        health = GetComponent<Health>();
        health.onHealthChange.AddListener(TurretTakesDamage);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(aimPoint, 0.5f);
        Gizmos.DrawLine(transform.position, aimPoint);
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        foreach(WeaponWielder weapon in weapons)
        {
            if(limitYaw)
            {
                Gizmos.color = Color.green;
                Vector3 leftExtent = 10.0f * (Quaternion.AngleAxis(-leftYawLimit, transform.up) * transform.forward);
                Vector3 rightExtent = 10.0f * (Quaternion.AngleAxis(rightYawLimit, transform.up) * transform.forward);
                Gizmos.DrawRay(transform.position, leftExtent);
                Gizmos.DrawRay(transform.position, rightExtent);
            }

            Gizmos.color = Color.red;
            Vector3 upExtent = 10.0f * (Quaternion.AngleAxis(-maxBarrelElevation, transform.right) * transform.forward);
            Vector3 downExtent = 10.0f * (Quaternion.AngleAxis(minBarrelDepression, transform.right) * transform.forward);
            Gizmos.DrawRay(weapon.transform.position, upExtent);
            Gizmos.DrawRay(weapon.transform.position, downExtent);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxEngagementRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxSightRange);
    }

    private void Update()
    {
        if (MenuController.Paused) return;

        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        GameObject target = PickTarget(potentialTargets);
        
        AimAndShoot(target);
    }

    protected void AimAndShoot(GameObject target)
    {
        float distanceToTarget = Vector3.Distance(transform.position, aimPoint);
        if(target != null)
        {
            Vector3 realTargetPosition = target.transform.position;

            CharacterController targetController = target.GetComponent<CharacterController>();
            Rigidbody targetRB = target.GetComponent<Rigidbody>();

            distanceToTarget = Vector3.Distance(transform.position, aimPoint);
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
        }
        else
        {
            aimPoint = idleAimPoint;
        }

        lastTarget = target;

        if(fireTimer < nonAutoFireRate) fireTimer += Time.deltaTime;

        RotateBaseTowardsTarget(aimPoint);
        RotateArmsToTarget(aimPoint);

        if(target != null
            && IsAimingAtTarget(aimPoint) 
            && distanceToTarget < maxEngagementRange)
        {
            FireWeapons();
        }
    }

    
    protected void FireWeapons()
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

    protected virtual void RotateBaseTowardsTarget(Vector3 targetPos)
    {
        Vector3 localTargetPos = transform.parent.InverseTransformPoint(targetPos);
        localTargetPos.y = 0.0f;

        Vector3 clampedLocalVec2Target = localTargetPos;
        if(limitYaw)
        {
            if (localTargetPos.y >= 0.0f)
                clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * maxBarrelElevation, float.MaxValue);
            else
                clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * minBarrelDepression, float.MaxValue);
        }
        
        Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
        Quaternion newRotation = Quaternion.RotateTowards(transform.localRotation, rotationGoal, yawRate * Time.deltaTime);
        transform.localRotation = newRotation;
    }

    protected void RotateArmsToTarget(Vector3 targetPos)
    {
        foreach(WeaponWielder weapon in weapons)
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(targetPos);
            localTargetPos.x = 0.0f;
            localTargetPos.y -= weapon.transform.localPosition.y;

            Vector3 clampedLocalVec2Target = localTargetPos;
            if (localTargetPos.y >= 0.0f)
                clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * maxBarrelElevation, float.MaxValue);
            else
                clampedLocalVec2Target = Vector3.RotateTowards(Vector3.forward, localTargetPos, Mathf.Deg2Rad * minBarrelDepression, float.MaxValue);

            Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
            
            Quaternion newRotation = Quaternion.RotateTowards(weapon.transform.localRotation, rotationGoal, barrelRate * Time.deltaTime);
            weapon.transform.localRotation = newRotation;
        }
    }

    protected bool IsAimingAtTarget(Vector3 targetPos)
    {
        foreach(WeaponWielder weapon in weapons)
        {
            Vector3 towardsTarget = (targetPos - weapon.transform.position).normalized;
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

    private void TurretTakesDamage(HealthChangeEvent e)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Enemies/Enemy_Damage", transform.position);
    }
}
