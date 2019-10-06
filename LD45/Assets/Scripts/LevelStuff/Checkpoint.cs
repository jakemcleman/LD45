using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    private static UnityEvent onCheckpoint;
    private static Checkpoint activeCheckpoint;

    private bool active;

    static Checkpoint()
    {
        onCheckpoint = new UnityEvent();
        activeCheckpoint = null;
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
