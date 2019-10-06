using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour
{
    public float DamagePerSecond = 50;

    private float gridSize = 5000;
    
    private void Update()
    {
        Health[] healthComponents = GameObject.FindObjectsOfType<Health>();

        foreach(Health healthComp in healthComponents)
        {
            if(healthComp.transform.position.y < transform.position.y)
            {
                healthComp.TakeDamage(DamagePerSecond * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.25f);
        Gizmos.DrawCube(transform.position - Vector3.up, new Vector3(gridSize, 2, gridSize));
    }
}
