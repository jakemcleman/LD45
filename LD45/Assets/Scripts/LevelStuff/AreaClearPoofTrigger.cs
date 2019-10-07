using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaClearPoofTrigger : PoofTrigger
{
    public Health[] AreaEnemies;
    public int acceptableRemaining = 0;

    private int enemiesKilled;

    private bool cleared;

    private void OnDrawGizmosSelected()
    {
        foreach(Health enemyHealth in AreaEnemies)
        {
            if(enemyHealth != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(enemyHealth.transform.position, 5.0f);
            }
        }
    }

    private void Start()
    {
        foreach(Health enemyHealth in AreaEnemies)
        {
            enemyHealth.onDeath.AddListener(OnEnemyDead);
        }

        enemiesKilled = 0;
        cleared = AreaEnemies.Length <= acceptableRemaining;
    }

    private void OnEnemyDead()
    {
        enemiesKilled++;

        int remaining = AreaEnemies.Length - enemiesKilled;

        Debug.LogFormat("Enemy killed, {0} remaining to advance", remaining);

        if(!cleared && remaining <= acceptableRemaining)
        {
            cleared = true;
            DoPoof();
        }
    }
}
