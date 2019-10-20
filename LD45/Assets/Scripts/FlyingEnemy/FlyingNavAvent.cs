﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingNavAvent : MonoBehaviour, IRidable
{

    public float maxSpeed = 10;
    public float acceleration = 1.0f;
    public float yAccelerationModifier = 0.5f;
    public float agentRadius = 1.0f;
    
    public bool decelerateEarly = false;

    public float arrivalRange = 1.0f;
    public Vector3 targetPosition;
    public bool retreat = false;

    private Vector3 velocity;

    public Vector3 Velocity 
    {
        get { return velocity; }
    }

    public Vector3 GetVelocity()
    {
        return Velocity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, agentRadius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPosition, 0.25f);
    }

    private Vector3 GetVelResultFromAccel(Vector3 direction)
    {
        Vector3 accel = direction * acceleration;
        accel.y += yAccelerationModifier;
        return Vector3.ClampMagnitude(velocity + accel, maxSpeed);
    }
    private void Accelerate(Vector3 direction)
    {
        velocity = GetVelResultFromAccel(direction);
    }

    private void ApplyVelocity()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private bool TestPotentialPath(Vector3 testVel)
    {
        Vector3 toTarget = targetPosition - transform.position;

        float distanceToTarget = toTarget.magnitude;
        float curSpeed = testVel.magnitude;
        float timeToTarget = distanceToTarget / curSpeed;
        float stopTime = curSpeed / acceleration;

        float castLength = Mathf.Min(distanceToTarget, stopTime * curSpeed);

        //Debug.DrawLine(transform.position, transform.position + (testVel.normalized * castLength), Color.grey, 0.5f);

        RaycastHit hit; 
        return Physics.SphereCast(transform.position, agentRadius, testVel.normalized, out hit, castLength, ~(1 << 8));
    }

    private void TestSolution(Vector3 adjustmentAcc, ref List<Vector3> optionsList)
    {
        Vector3 resultVel = GetVelResultFromAccel(adjustmentAcc);
        
        if(!TestPotentialPath(resultVel)) optionsList.Add(adjustmentAcc);
    }

    private float ScoreSolution(Vector3 adjustmentAcc)
    {
        Vector3 toTarget = targetPosition - transform.position;

        float closenessToDesiredDirection = Vector3.Dot(GetVelResultFromAccel(adjustmentAcc).normalized, toTarget.normalized);
        float verticalMovementCost = adjustmentAcc.y;

        return (closenessToDesiredDirection * 3) + (verticalMovementCost * -3);
    }

    private void Update()
    {
        if (MenuController.Paused) return;
        
        // If we're retreating, reverse the vector st we move away instead of
        // forward
        Vector3 toTarget = (retreat) ? transform.position - targetPosition :
         targetPosition - transform.position;

        float distanceToTarget = toTarget.magnitude;
        float curSpeed = velocity.magnitude;
        float timeToTarget = distanceToTarget / curSpeed;
        float stopTime = curSpeed / acceleration;

        RaycastHit hit;
        if(TestPotentialPath(velocity))
        {
            // There is a potential collision ahead
            // Check up/down/left/right for best adjustment to path to avoid collision
            List<Vector3> possibleAdjustments = new List<Vector3>();

            // Try slowing down (give more time to manuever for next frame)
            TestSolution(-toTarget.normalized, ref possibleAdjustments);
            // Try going right
            TestSolution(Vector3.Cross(velocity.normalized, Vector3.up), ref possibleAdjustments);
            // Try going left
            TestSolution(-Vector3.Cross(velocity.normalized, Vector3.up), ref possibleAdjustments);
            // Try all 6 basic directions
            TestSolution(Vector3.up, ref possibleAdjustments);
            TestSolution(Vector3.down, ref possibleAdjustments);
            TestSolution(Vector3.left, ref possibleAdjustments);
            TestSolution(Vector3.right, ref possibleAdjustments);
            TestSolution(Vector3.forward, ref possibleAdjustments);
            TestSolution(Vector3.back, ref possibleAdjustments);

            if(possibleAdjustments.Count == 0) 
            {
                Debug.Log("NO WAY TO AVOID COLLISION WAS FOUND PANIC");
                Accelerate(-velocity.normalized);
            }
            else
            {
                Vector3 bestSolution = Vector3.zero;
                float bestScore = float.MinValue;

                foreach(Vector3 solution in possibleAdjustments)
                {
                    float score = ScoreSolution(solution);
                    if(score > bestScore)
                    {
                        bestSolution = solution;
                        bestScore = score;
                    }
                }


                //Debug.Log("Best avoidance solution is " + bestSolution);
                Accelerate(bestSolution);
            }
        }
        else
        {
            if(distanceToTarget > arrivalRange)
            {
                // Tuned to deliberately overshoot a little
                if(!decelerateEarly || stopTime <= timeToTarget) Accelerate(toTarget.normalized);
                else Accelerate(-toTarget.normalized);
            }
            else
            {
                Accelerate(-velocity.normalized);
            }
        }
        

        ApplyVelocity();
    }
}