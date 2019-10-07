using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainMenuWhenYouTouchTheKeyboard : MonoBehaviour
{
    private void Update()
    {
        if(Input.anyKeyDown)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
