using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string music_event;
    FMOD.Studio.EventInstance music;

    // Start is called before the first frame update
    void Start()
    {
        FMODUnity.RuntimeManager.PlayOneShot(music_event, this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
