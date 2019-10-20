using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EplodedDebris : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    public float cleanupTime = 10;
    public float throwForce = 10;

    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.AddForce(Random.insideUnitSphere * throwForce);
        }

        Checkpoint.onReset.AddListener(OnResetToCheckpoint);

        Destroy(gameObject, cleanupTime);
    }

    private void OnResetToCheckpoint()
    {
        Destroy(gameObject);
    }
}
