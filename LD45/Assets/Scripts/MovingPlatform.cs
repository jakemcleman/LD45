using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IRidable
{
#pragma warning disable 0649
    [SerializeField]
    Transform initialLocation;
    Vector3 initialPos;

    [SerializeField]
    Transform endLocation;
    Vector3 endPos;

    [SerializeField]
    AnimationCurve t_curve;

    bool returning = false;

    [SerializeField]
    float oneWayTime;
    float timer;

    [SerializeField]
    bool isTrigger;
    bool launch = false;

#pragma warning restore 0649

    private Vector3 vel;
    public Vector3 velocity
    {
        get => vel;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPos = initialLocation.position;
        endPos = endLocation.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (!Application.isPlaying)
        {
            Gizmos.DrawCube(endLocation.position, transform.localScale);
            Gizmos.DrawLine(initialLocation.position, endLocation.position);
        }
        else
        {
            Gizmos.DrawCube(initialPos, transform.localScale);
            Gizmos.DrawCube(endPos, transform.localScale);
            Gizmos.DrawLine(initialPos, endPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTrigger && other.gameObject.tag == "Player")
        {
            launch = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTrigger && launch)
        {
            MoveUpdate();
        }
        else if (!isTrigger)
        {
            MoveUpdate();
        }
    }

    void MoveUpdate()
    {
        timer += Time.deltaTime;
        if (timer > oneWayTime)
        {
            timer = 0;
            returning = !returning;

            //If returned.
            if (returning == false)
            {
                launch = false;
            }
        }
        float t = timer / oneWayTime;
        t = t_curve.Evaluate(t);
        Vector3 prevPos = transform.position;
        if (!returning)
        {
            transform.position = Vector3.Lerp(initialPos, endPos, t);
        }
        else
        {
            transform.position = Vector3.Lerp(endPos, initialPos, t);
        }
        vel = (transform.position - prevPos) / Time.deltaTime;
    }
}
