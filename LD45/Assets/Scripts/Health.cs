using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public struct HealthChangeEvent
{
    public float amount;
}

[System.Serializable]
public class OnHealthChangeEvent : UnityEvent<HealthChangeEvent>
{
}

public class Health : MonoBehaviour
{
    public OnHealthChangeEvent onHealthChange;
    public UnityEvent onDeath;

    public float maxHealth = 100;
    public float damageMultiplier = 1;

    private float curHealth;
    public GameObject damageIndicatorPrefab;

    public float CurrentHealth 
    {
        get { return curHealth; }
    }

    public float HealthRatio 
    {
        get { return curHealth / maxHealth; }
    }

    private void Start()
    {
        curHealth = maxHealth;
        HealthChangeEvent healthChange;
        healthChange.amount = 0;
        onHealthChange.Invoke(healthChange);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(10.0f);
        if (Input.GetKeyDown(KeyCode.H))
            Heal(10.0f);
    }

    /*
     * Deal an amount of damage, returns true if this deals a killing blow, false otherwise
     */
    public bool TakeDamage(float amount) 
    {
        if (Debug.isDebugBuild) 
        {
            if(amount < 0) Debug.LogWarningFormat("Requested negative damage {0} to be dealt to {1}}", amount, gameObject.name);
        }

        HealthChangeEvent healthChange;
        healthChange.amount = amount * -damageMultiplier;
        onHealthChange.Invoke(healthChange);

        curHealth -= amount * damageMultiplier;

        if(damageIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(damageIndicatorPrefab, transform.position, Quaternion.identity) as GameObject;
            float damage = Mathf.Min(10000, amount * damageMultiplier);
            indicator.GetComponent<DamageIndicatorMotion>().velocity = (damage / 600) * Vector3.up;
            indicator.GetComponent<TextMesh>().text = damage.ToString();
        }
        
        Health[] parentHealths = GetComponentsInParent<Health>();

        foreach(Health parentHealth in parentHealths)
        {
            if(parentHealth != null && parentHealth != this) parentHealth.TakeDamage(amount / 2);
        } 
            

        if(curHealth <= 0)
        {
            // TODO: die
            onDeath.Invoke();
            return true;
        }
        else 
        {
            return false;
        }
    }

    /*
     * Heal an amount of damage, returns a boolean of if any healing was able to be done
     * (false if no healing was requested, or if health is already max)
     */
    public bool Heal(float amount) 
    {
        // Check if healing is possible to avoid wasting health packs
        if(amount == 0 || curHealth >= maxHealth)
        {
            return false;
        }
            

        if (Debug.isDebugBuild) 
        {
            if(amount < 0) Debug.LogWarningFormat("Requested negative healing {0} to be done to {1}}", amount, gameObject.name);
        }

        curHealth += amount;

        HealthChangeEvent healthChange;
        healthChange.amount = amount;
        onHealthChange.Invoke(healthChange);
        Debug.Log("Heal "+healthChange.amount);

        if (curHealth > maxHealth) curHealth = maxHealth;

        return true;
    }
}
