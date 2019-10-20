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

    private Vector3 startPos;

    private TextMesh textMesh;

    void Start()
    {
        startPos = transform.position;

        textMesh = GetComponent<TextMesh>();
    }

    void Update()
    {
        timeCounter += Time.deltaTime;
        Vector3 endPos = startPos + (maxTime * velocity);
        float t = timeCounter / maxTime;

        transform.position = Vector3.Lerp(startPos, endPos, t);
        
        if (timeCounter >= maxTime)
        {
            Destroy(gameObject);
        }
    }
}
