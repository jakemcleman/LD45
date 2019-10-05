using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour
{
    public string TestEventName;
    public string VCA_SFX_Path;
    public string VCA_Music_Path;

    private float sfx_volume = 0.8f;
    private float mus_volume = 0.8f;

    FMOD.Studio.VCA VCA_SFX;
    FMOD.Studio.VCA VCA_Music;

    // Start is called before the first frame update
    void Start()
    {
        VCA_SFX = FMODUnity.RuntimeManager.GetVCA(VCA_SFX_Path);
        VCA_Music = FMODUnity.RuntimeManager.GetVCA(VCA_Music_Path);
    }

    // Update is called once per frame
    void Update()
    {
        // Test event
        if (Input.GetKeyDown(KeyCode.Alpha9))
            FMODUnity.RuntimeManager.PlayOneShot(TestEventName, this.transform.position);

        // Volume control
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            sfx_volume = Mathf.Clamp(sfx_volume - 0.1f, 0.0f, 1.0f);
            VCA_SFX.setVolume(sfx_volume);
            VCA_Music.setVolume(sfx_volume);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            sfx_volume = Mathf.Clamp(sfx_volume + 0.1f, 0.0f, 1.0f);
            VCA_SFX.setVolume(sfx_volume);
            VCA_Music.setVolume(sfx_volume);
        }
    }
}
