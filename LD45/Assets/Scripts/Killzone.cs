using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Killzone : MonoBehaviour
{
    public float DamagePerSecond = 50;

    public bool DestroyAll = false;
    
    void OnTriggerStay(Collider col)
    {
        Health healthComp = col.GetComponent<Health>();
        if(healthComp != null)
        {
            healthComp.TakeDamage(DamagePerSecond * Time.deltaTime);
        }
        else if(DestroyAll)
        {
            Destroy(col.gameObject);
        }
    }
}
