using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string health_event;
    FMOD.Studio.EventInstance health_state;

    private Health health;

    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
        health_state = FMODUnity.RuntimeManager.CreateInstance(health_event);
        health_state.start();
    }

    // Update is called once per frame
    void Update()
    {
        health_state.setParameterByName("Player_Health", health.HealthRatio);
    }
}
