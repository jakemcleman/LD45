using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100;
    public float damageMultiplier = 1;

    private float curHealth;

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

        curHealth -= amount * damageMultiplier;

        if(curHealth <= 0)
        {
            // TODO: die
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
        if(amount == 0 || curHealth >= maxHealth) return false;

        if (Debug.isDebugBuild) 
        {
            if(amount < 0) Debug.LogWarningFormat("Requested negative healing {0} to be done to {1}}", amount, gameObject.name);
        }

        curHealth += amount;

        if (curHealth > maxHealth) curHealth = maxHealth;

        return true;
    }
}
