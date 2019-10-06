using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private GameObject player;
    private GameObject canvas;

    public string nextScene;
    private Scene curScene;

    private bool isLoaded;

    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        canvas = GameObject.FindGameObjectWithTag("UI");

        isLoaded = false;
    }

    public void StartLoad(bool doUnload)
    {      
        if (!isLoaded)
        {
            coroutine = LoadSceneAsync(doUnload);
            StartCoroutine(coroutine);
        }
        else
        {
            Debug.LogError("New scene has already been loaded");
        }
    }

    public void UnLoadScene()
    {
        if (isLoaded)
        {
            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName(nextScene));
            SceneManager.MoveGameObjectToScene(canvas, SceneManager.GetSceneByName(nextScene));
            SceneManager.UnloadSceneAsync(curScene);
        }
        else
        {
            Debug.LogError("New scene is not yet loaded, don't unload the scene till a new scene is loaded :(");
        }
    }

    IEnumerator LoadSceneAsync(bool doUnload)
    {
        curScene = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isLoaded = true;
    }
}
