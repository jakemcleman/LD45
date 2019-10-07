using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string music_event;
    FMOD.Studio.EventInstance music;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += KillMusic;
        if (!SceneLoader.gameMusicHasStarted)
        {
            string name = SceneManager.GetActiveScene().name;

            if (name != "MainMenu" && name != "Nothing")
            {
                music = FMODUnity.RuntimeManager.CreateInstance(music_event);
                music.start();
                SceneLoader.gameMusicHasStarted = true;
            }
        }
    }

    void KillMusic(Scene s1, Scene s2)
    {
        string name = SceneManager.GetActiveScene().name;

        if (name == "MainMenu")
            music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
