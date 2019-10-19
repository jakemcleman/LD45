using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointReset : MonoBehaviour
{
    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool dead;
        public bool poofed;
        public float health;
    }

    State resetToState;

    private ObjectPoofer objectPoofer;
    private Health objectHealth;

    public bool hasDied = false;

    private void Start()
    {
        objectPoofer = GetComponent<ObjectPoofer>();
        objectHealth = GetComponent<Health>();

        SetResetScateToCurrent();

        Checkpoint.onCheckpoint.AddListener(OnCheckpointHit);
        Checkpoint.onReset.AddListener(OnResetToCheckpoint);
    }

    public void SetDead()
    {
        hasDied = true;

        SetAllOtherComponentsActive(false);
    }

    public void SetResetScateToCurrent()
    {
        resetToState = CaptureState();
    }

    private void SetAllOtherComponentsActive(bool activeness)
    {
        MonoBehaviour[] allComponents = GetComponentsInChildren<MonoBehaviour>();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach(MonoBehaviour comp in allComponents)
        {
            if(!(comp is CheckpointReset)) comp.enabled = activeness;
        }
        foreach(Renderer rend in allRenderers)
        {
            rend.enabled = activeness;
        }
        foreach(Collider col in allColliders)
        {
            col.enabled = activeness;
        }
    }

    private void OnCheckpointHit()
    {
        SetResetScateToCurrent();

        if(resetToState.dead)
        {
            Destroy(gameObject);
        }
    }

    private void OnResetToCheckpoint()
    {
        ApplyState(resetToState);
    }

    private State CaptureState()
    {
        State state = new State();
        state.position = transform.localPosition;
        state.rotation = transform.localRotation;
        state.dead = hasDied;

        if(objectPoofer != null)
        {
            state.poofed = objectPoofer.PoofedIn;
        }
        else
        {
            state.poofed = true;
        }

        if(objectHealth != null)
        {
            state.health = objectHealth.CurrentHealth;
        }
        else
        {
            state.health = 1;
        }

        return state;
    }

    private void ApplyState(State toApply)
    {
        if(toApply.dead)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.localPosition = toApply.position;
            transform.localRotation = toApply.rotation;
            
            if(objectPoofer != null && !toApply.poofed)
            {
                objectPoofer.Reset();
            }

            if(objectHealth != null)
            {
                objectHealth.CurrentHealth = toApply.health;
            }

            SetAllOtherComponentsActive(true);
        }
    }
}
