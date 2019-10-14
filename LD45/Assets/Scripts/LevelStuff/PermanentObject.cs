using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermanentObject : MonoBehaviour
{
    public string objectType  = "PlsKill";
    public bool deleteOverride = false;

    private SceneLoader SL;

    // Start is called before the first frame update
    void Start()
    {
        SL = GameObject.FindObjectOfType<SceneLoader>();

        if (SL != null)
        {
            SL.RegisterDestroy(this.gameObject);
        }
        else
        {
            Debug.LogError("Couldn't find SceneLoader :(");
        }
    }
}
