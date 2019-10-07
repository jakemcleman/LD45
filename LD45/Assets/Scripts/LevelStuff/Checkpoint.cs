﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    private static UnityEvent onCheckpoint;
    private static Checkpoint activeCheckpoint;

    private bool active;

    public GameObject InactiveCP;
    public GameObject ActiveCP;

    static Checkpoint()
    {
        onCheckpoint = new UnityEvent();
        activeCheckpoint = null;
    }

    public bool getActive()
    {
        return active;
    }

    private void Start()
    {
        active = false;

        onCheckpoint.AddListener(OnCheckpoint);
    }

    private void OnCheckpoint()
    {
        active = activeCheckpoint == this;
    }
      
    private void OnTriggerEnter(Collider col)
    {
        if(!active)
        {
            if(col.tag == "Player")
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Env/Checkpoint_Activate", col.transform.position);
                ActivateCheckpoint(col.gameObject);
            }
        }
    }

    private void ActivateCheckpoint(GameObject player)
    {
        player.GetComponent<PlayerHealth>().SetSpawn(transform.position, transform.forward);
        player.GetComponent<MovementController>().ResetState();
        activeCheckpoint = this;
        onCheckpoint.Invoke();

        Debug.Log("Checkpoint Reached");
    }
}
