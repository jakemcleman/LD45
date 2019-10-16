﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static bool gameMusicHasStarted = false;

    public int firstLevelIndex;
    public int lastLevelIndex;
    private int curSceneIndex;
    private int nextSceneIndex;

    private GameObject player;
    private GameObject canvas;
    private GameObject curlight;
    private GameObject kz;
    private GameObject eventSystem;

    private List<GameObject> permanentObjects = new List<GameObject>(); 

    public string nextSceneName;

    private IEnumerator coroutine;

    public static int CurrentScene
    {
        get 
        {
            return FindObjectOfType<SceneLoader>().curSceneIndex;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //permanentObjects.Add(this.gameObject);

        //player = GameObject.FindGameObjectWithTag("Player");
        //canvas = GameObject.FindGameObjectWithTag("UI");
        //curlight = GameObject.FindObjectOfType<Light>().gameObject;
        //kz = GameObject.FindObjectOfType<Killzone>().gameObject;
        //eventSystem = GameObject.FindObjectOfType<EventSystem>().gameObject;

        ResetCurIndex();
    }

    public void RegisterDestroy(GameObject newObj)
    {
        PermanentObject newObjPO = newObj.GetComponent<PermanentObject>();

        if (newObjPO.objectType == "PlsKill") return;

        foreach (GameObject oldObj in permanentObjects)
        {
            PermanentObject oldObjPO = oldObj.GetComponent<PermanentObject>();

            if (newObjPO.objectType == oldObjPO.objectType)
            {
                if (newObjPO.deleteOverride == true)
                {
                    Debug.Log("Override; Deleteing " + oldObj + " Adding " + newObj);
                    Destroy(oldObj);
                    permanentObjects.Remove(oldObj);
                    permanentObjects.Add(newObj);
                    return;
                }
                else
                {
                    Debug.Log("Found Duplicate; Deleting " + newObj);
                    Destroy(newObj);
                    return;
                }
            }
        }

        Debug.Log("No Duplicate; Adding " + newObj);
        permanentObjects.Add(newObj);
    }

    public void ResetCurIndex()
    {
        curSceneIndex = firstLevelIndex;
        Debug.Log("Setting curSceneIndex to " + curSceneIndex);
    }

    public void StartLoad(GameObject trigger, bool doUnload, string sceneName = null)
    {      
        coroutine = LoadSceneAsync(trigger, doUnload, sceneName);
        StartCoroutine(coroutine);
        trigger.SetActive(false);
    }

    public void UnLoadScene(GameObject trigger, string sceneName = null)
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
                if (cp.scene == unloadScene && cp.GetComponent<Checkpoint>().getActive() == true)
                {
                    Debug.LogError("There is still an active checkpoint " + cp.name + " in this scene " + unloadScene.buildIndex + "  CurSceneIndex: " + curSceneIndex + "; Active scene: " + SceneManager.GetActiveScene().buildIndex);
                    return;
                }
            }

            SceneManager.UnloadSceneAsync(unloadScene);
            trigger.SetActive(false);
        }
    }

    private void MoveToScene(Scene nextScene)
    {
        AnalyticsReporter.ReportLevelCompleted();

        //Let's go find any duplicate players or lights or canvas and destroy them!!!
        //foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        //{
        //    if (p.scene == nextScene) Destroy(p);
        //}

        //foreach (GameObject c in GameObject.FindGameObjectsWithTag("UI"))
        //{
        //    if (c.scene == nextScene) Destroy(c);
        //}

        //EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();
        //foreach (EventSystem es in eventSystems)
        //{
        //    if (es.gameObject.scene == nextScene) Destroy(es.gameObject);
        //}

        //Light[] lights = GameObject.FindObjectsOfType<Light>();
        //foreach (Light l in lights)
        //{
        //    if (l.gameObject.scene == nextScene) Destroy(l.gameObject);
        //}

        //Killzone[] kzs = GameObject.FindObjectsOfType<Killzone>();
        //foreach (Killzone k in kzs)
        //{
        //    if (k.gameObject.scene == nextScene) Destroy(k.gameObject);
        //}

        //foreach (GameObject g in GameObject.FindGameObjectsWithTag("PlsKill"))
        //{
        //    if (g.gameObject.scene == nextScene) Destroy(g);
        //}

        curSceneIndex = nextSceneIndex;

        //Activates the newly loaded scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(curSceneIndex));
        
        foreach (GameObject go in permanentObjects)
        {
            if (go.GetComponent<PermanentObject>().objectType == "PlsKill") Destroy(go);
            else SceneManager.MoveGameObjectToScene(go, nextScene);
        }

        //SceneManager.MoveGameObjectToScene(player, nextScene);
        //SceneManager.MoveGameObjectToScene(canvas, nextScene);
        //SceneManager.MoveGameObjectToScene(eventSystem, nextScene);
        //SceneManager.MoveGameObjectToScene(curlight, nextScene);
        //SceneManager.MoveGameObjectToScene(kz, nextScene);
        //SceneManager.MoveGameObjectToScene(this.gameObject, nextScene);

       
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
        Debug.Log("Map rotation vector: " + rotateVector.eulerAngles);
        
        Vector3 moveVector = newStart.transform.position - curEnd.transform.position;
        Debug.Log("Start point: " + newStart.transform.position + " End point: " + curEnd.transform.position + " Move vector: " + moveVector);
        newMap.transform.position -= moveVector;
    }

    IEnumerator LoadSceneAsync(GameObject trigger, bool doUnload, string sceneName = null)
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

        Debug.Log("Loading scene: " + SceneManager.GetSceneByBuildIndex(nextSceneIndex).name + " At index: " + nextSceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        MoveNewMap();
        MoveToScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (doUnload) UnLoadScene(trigger);
    }
}
