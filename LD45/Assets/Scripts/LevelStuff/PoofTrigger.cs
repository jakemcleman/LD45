using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoofTrigger : MonoBehaviour
{
    public GameObject[] poofs;

    public enum PTAction
    {
        PoofIn,
        PoofOut
    }

    public PTAction Action;

    private bool active;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        foreach(GameObject poof in poofs)
        {
            if(poof != null)
            {
                Gizmos.DrawSphere(poof.transform.position, 10.0f);
                Gizmos.DrawLine(poof.transform.position, poof.transform.position + poof.GetComponent<ObjectPoofer>().poofFromVector);
            }
        }
    }

    protected void DoPoof()
    {
        switch (Action)
            {
                case PTAction.PoofIn:
                    if (!active)
                    {
                        if (poofs.Length > 0)
                        {
                            Debug.Log("Poofing in " + poofs.Length + " objects");
                            foreach (GameObject poof in poofs)
                            {
                                poof.SetActive(true);
                                poof.GetComponent<ObjectPoofer>().StartPoofIn();
                            }
                            active = true;
                        }
                    }
                    break;

                case PTAction.PoofOut:
                    if (!active)
                    {
                        if (poofs.Length > 0)
                        {
                        Debug.Log("Poofing out " + poofs.Length + " objects");
                        foreach (GameObject poof in poofs)
                            {
                                poof.GetComponent<ObjectPoofer>().StartPoofOut();
                            }
                            active = true;
                        }
                    }
                    break;
            } 
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
             DoPoof();
        }
    }
}
