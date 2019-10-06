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

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            switch (Action)
            {
                case PTAction.PoofIn:
                    if (!active)
                    {
                        if (poofs.Length > 0)
                        {
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
    }
}
