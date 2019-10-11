using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class SplashScreenController : MonoBehaviour
{
    public int FirstFrame = 0;
    public float FadeTime = 0.5f;
    private float fadeTimer;

    private VideoPlayer player;
    private bool playing;
    private bool advancing;

    private void Start()
    {
        fadeTimer = 0;
        playing = false;
        advancing = false;
        player = GetComponent<VideoPlayer>();
        player.Prepare();
        player.targetCameraAlpha = 0;
        player.frame = FirstFrame;
    }
    private void Update()
    {
        if(Input.anyKeyDown && !advancing)
        {
            AdvanceToNextScene();
        }

        if(player.isPrepared)
        {
            if(fadeTimer <= FadeTime)
            {
                fadeTimer += Time.deltaTime;
                player.targetCameraAlpha = fadeTimer / FadeTime;
                player.Play();
                player.Pause();
            }
            else if(!playing)
            {
                playing = true;
                player.targetCameraAlpha = 1;
                player.Play();
            }
            else
            {
                if(player.time >= player.length - FadeTime && !advancing)
                {
                    AdvanceToNextScene();
                }
            }
        }
    }

    private void AdvanceToNextScene()
    {
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeOut()
    {
        float time = 0;
        advancing = true;

        float startAlpha = player.targetCameraAlpha;

        while(time < FadeTime * startAlpha)
        {
            time += Time.deltaTime;
            float t = (time / (FadeTime * startAlpha));
            player.targetCameraAlpha = startAlpha - t;
            yield return null;
        }

        SceneManager.LoadScene("MainMenu");
    }
}
