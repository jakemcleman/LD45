using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingNavAvent : MonoBehaviour
{

    public float maxSpeed = 10;
    public float acceleration = 1.0f;
    public float yAccelerationModifier = 0.5f;
    public float agentRadius = 1.0f;

    public float arrivalRange = 1.0f;
    public Vector3 targetPosition;

    private Vector3 velocity;

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

    private void Accelerate(Vector3 direction)
    {
        Vector3 accel = direction * acceleration;
        accel.y += yAccelerationModifier;

        velocity += accel;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    private void ApplyVelocity()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void Update()
    {
        Vector3 toTarget = targetPosition - transform.position;

        float distanceToTarget = toTarget.magnitude;
        float curSpeed = velocity.magnitude;
        float timeToTarget = distanceToTarget / curSpeed;
        float stopTime = curSpeed / acceleration;

        if(distanceToTarget > arrivalRange)
        {
            if(stopTime <= timeToTarget) Accelerate(toTarget.normalized);
            else Accelerate(-toTarget.normalized);
        }
        else
        {
            Accelerate(-velocity.normalized);
        }

        ApplyVelocity();
    }
}
