using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour
{
    public float DamagePerSecond = 250;

    public const float killHeight = -10;

    private float gridSize = 5000;
    
    private void Update()
    {
        if (MenuController.Paused) return;
        
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
        Gizmos.DrawCube((killHeight - 1) * Vector3.up, new Vector3(gridSize, 2, gridSize));
    }
}
