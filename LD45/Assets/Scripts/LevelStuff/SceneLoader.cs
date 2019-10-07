using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int firstLevelIndex;
    public int lastLevelIndex;
    private int curSceneIndex;
    private int nextSceneIndex;

    private GameObject player;
    private GameObject canvas;
    private GameObject light;
    //private GameObject newMap;

    public string nextSceneName;

    private bool isLoaded;

    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        canvas = GameObject.FindGameObjectWithTag("UI");
        light = GameObject.FindObjectOfType<Light>().gameObject;

        isLoaded = false;
    }

    public void StartLoad(bool doUnload, string sceneName = null)
    {      
        if (!isLoaded)
        {
            coroutine = LoadSceneAsync(doUnload, sceneName);
            StartCoroutine(coroutine);
        }
        else
        {
            Debug.LogError("New scene has already been loaded");
        }
    }

    public void UnLoadScene(string sceneName = null)
    {
        Scene unloadScene;
        if (sceneName == null)
        {
            unloadScene = SceneManager.GetSceneByBuildIndex(curSceneIndex - 1);
        }
        else
        {
            unloadScene = SceneManager.GetSceneByName(sceneName);
        }

        if (unloadScene.isLoaded)
        {
            foreach (GameObject cp in GameObject.FindGameObjectsWithTag("Checkpoint"))
            {
                if (cp.scene == SceneManager.GetSceneByBuildIndex(curSceneIndex) && cp.GetComponent<Checkpoint>().getActive() == true)
                {
                    Debug.LogError("There is still an active checkpoint in this scene");
                    return;
                }
            }

            SceneManager.UnloadSceneAsync(unloadScene);
        }
    }

    private void MoveToScene(Scene nextScene)
    {
        //Let's go find any duplicate players or lights or canvas and destroy them!!!
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.scene == nextScene) Destroy(p);
        }

        foreach (GameObject c in GameObject.FindGameObjectsWithTag("UI"))
        {
            if (c.scene == nextScene) Destroy(c);
        }

        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.gameObject.scene == nextScene) Destroy(l.gameObject);
        }

        curSceneIndex = nextSceneIndex;

        //Activates the newly loaded scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(curSceneIndex));
        
        //Insert any root level objects that need to move scenes here
        SceneManager.MoveGameObjectToScene(player, nextScene);
        SceneManager.MoveGameObjectToScene(canvas, nextScene);
        SceneManager.MoveGameObjectToScene(light, nextScene);
        SceneManager.MoveGameObjectToScene(this.gameObject, nextScene);
    }

    private void MoveNewMap()
    {
        Debug.Log("Moving new map");
        GameObject curMap = null;
        GameObject newMap = null;
        GameObject curEnd = null;
        GameObject newStart = null;

        Scene curScene = SceneManager.GetSceneByBuildIndex(curSceneIndex);
        Scene nextScene = SceneManager.GetSceneByBuildIndex(nextSceneIndex);

        //Find the maps
        foreach (GameObject map in GameObject.FindGameObjectsWithTag("Map"))
        {
            if (map.scene == nextScene)
            {
                newMap = map;
            }
            else if (map.scene == curScene)
            {
                curMap = map;
            }
        }
        if (newMap == null) Debug.LogError("Could not find the new map");
        if (curMap == null) Debug.LogError("Could not find the current map");

        //Find the start/end points
        LevelStartEndPoint[] points = GameObject.FindObjectsOfType<LevelStartEndPoint>();
        foreach (LevelStartEndPoint p in points)
        {
            if (p.pointType == LevelStartEndPoint.LevelPointType.End && p.gameObject.scene == curScene)
            {
                curEnd = p.gameObject;
            }
            else if (p.pointType == LevelStartEndPoint.LevelPointType.Start && p.gameObject.scene == nextScene)
            {
                newStart = p.gameObject;
            }
        }
        if (curEnd == null) Debug.LogError("Could not find the current level's endpoint");
        if (newStart == null) Debug.LogError("Could not find the new level's startpoint");

        Quaternion rotateVector = Quaternion.FromToRotation(curEnd.GetComponent<LevelStartEndPoint>().direction, newStart.GetComponent<LevelStartEndPoint>().direction);
        newMap.transform.rotation = rotateVector;
        
        Vector3 moveVector = newStart.transform.position - curEnd.transform.position;
        Debug.Log("Start point: " + newStart.transform.position + " End point: " + curEnd.transform.position + " Move vector: " + moveVector);
        newMap.transform.position -= moveVector;
    }

    IEnumerator LoadSceneAsync(bool doUnload, string sceneName = null)
    {
        AsyncOperation asyncLoad;
        
        if (sceneName == null)
        {
            nextSceneIndex = curSceneIndex + 1;
            asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive);
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            nextSceneIndex = SceneManager.GetSceneByName(nextSceneName).buildIndex;
        }      

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isLoaded = true;

        MoveNewMap();
        MoveToScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));

        if (doUnload) UnLoadScene();
    }
}
