using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour
{
    public string TestEventName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
            FMODUnity.RuntimeManager.PlayOneShot(TestEventName, this.transform.position);
    }
}
