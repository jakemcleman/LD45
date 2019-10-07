using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorMotion : MonoBehaviour
{
    public float maxTime = 2.0f;
    public float deceleration = 0.4f;
    public float cutoffVelocity = 0.2f;
    private float timeCounter = 0.0f;
    public Vector3 velocity = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       gameObject.transform.position = gameObject.transform.position + velocity * Time.deltaTime;
       velocity = velocity - (deceleration * Time.deltaTime) * velocity.normalized;
       if (velocity.magnitude < cutoffVelocity)
       {
           velocity = Vector3.zero;
       }
       timeCounter += Time.deltaTime;
       if (timeCounter >= maxTime)
       {
           Destroy(gameObject);
       }
    }
}
