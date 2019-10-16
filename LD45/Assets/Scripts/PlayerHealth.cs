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

    private ProgressBar healthBar;

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

        healthBar = GameObject.Find("FullHealthBar").GetComponent<ProgressBar>();
        healthBar.Fill = health.HealthRatio;

        SetSpawnToCurrentState();
    }
    public bool Heal(float amount)
    {
        return health.Heal(amount);
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

        healthBar.Fill = health.HealthRatio;
    }

    private void OnDeath()
    {
        AnalyticsReporter.ReportPlayerDied(transform.position);
        Respawn();
    }

    public void Respawn()
    {
        health.Heal(health.maxHealth);
        health_state.setParameterByName("Player_Health", 1.0f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Env/Checkpoint_Respawn", transform.position);
        
        CharacterController cc = GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = respawnPosition;
        transform.forward = respawnFacing;
        cc.enabled = true;

        cc.SimpleMove(Vector3.zero);
    }
}
