using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string health_event;
    FMOD.Studio.EventInstance health_state;

    private Health health;

    private Vector3 respawnPosition;
    private Vector3 respawnFacing;

    // Start is called before the first frame update
    private void Start()
    {
        health = GetComponent<Health>();
        health.onHealthChange.AddListener(OnHealthChange);
        health.onDeath.AddListener(OnDeath);
        health_state = FMODUnity.RuntimeManager.CreateInstance(health_event);
        health_state.setParameterByName("Player_Health", health.HealthRatio);
        health_state.start();

        SetSpawnToCurrentState();
    }

    public void SetSpawn(Vector3 position, Vector3 facing)
    {
        respawnPosition = position;
        respawnFacing = facing;
    }

    public void SetSpawnToCurrentState()
    {
        SetSpawn(transform.position, transform.forward);
    }

    private void OnHealthChange(HealthChangeEvent e)
    {
        health_state.setParameterByName("Player_Health", health.HealthRatio);
    }

    private void OnDeath()
    {
        Respawn();
    }

    private void Respawn()
    {
        health.Heal(health.maxHealth);
        health_state.setParameterByName("Player_Health", 1.0f);
        transform.position = respawnPosition;
        transform.forward = respawnFacing;
    }
}
