using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoofTrigger : MonoBehaviour
{
    public GameObject[] poofs;

    private bool active;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!active)
        {
            if (col.tag == "Player")
            {
                if (poofs != null)
                {
                    foreach (GameObject poof in poofs)
                    {
                        poof.GetComponent<ObjectPoofer>().StartPoofIn();
                    }

                    active = true;
                }
            }
        }
    }
}
