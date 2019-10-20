using System.Collections;
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

    private static List<GameObject> permanentObjects = new List<GameObject>(); 

    public string nextSceneName;

    private IEnumerator coroutine;

    public static int CurrentScene
    {
        get 
        {
            return FindObjectOfType<SceneLoader>().curSceneIndex;
        }
    }

    public static void ClearPOs()
    {
        if (permanentObjects != null)
        {
            permanentObjects = new List<GameObject>();
        }
        else
        {
            permanentObjects.Clear();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetCurIndex();
    }

    public void RegisterDestroy(GameObject newObj)
    {
        Debug.Log(newObj + " has started registering");

        PermanentObject newObjPO = newObj.GetComponent<PermanentObject>();

        if (newObjPO.objectType == "PlsKill")
        {
            Debug.Log("Found PlzKill object: " + newObj);
            permanentObjects.Add(newObj);
            return;
        }

        foreach (GameObject oldObj in permanentObjects)
        {
            if (oldObj == null)
            {
                Debug.LogWarning("Dead object still in permanent list, deleting " + oldObj);
                permanentObjects.Remove(oldObj);
                return;
            }

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
        Debug.Log("Starting load");
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

        curSceneIndex = nextSceneIndex;

        //Activates the newly loaded scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(curSceneIndex));
        
        //Delete any PlsKill Objects
        for (int i = 0; i < permanentObjects.Count; i++)
        {
            if (permanentObjects[i] == null)
            {
                Debug.LogWarning("Dead object still in permanent list");
            }
            else if (permanentObjects[i].GetComponent<PermanentObject>().objectType == "PlsKill")
            {
                Debug.Log("Destroying PlsKill object " + permanentObjects[i]);
                Destroy(permanentObjects[i]);
                permanentObjects.RemoveAt(i);
                i--;
            }
            else SceneManager.MoveGameObjectToScene(permanentObjects[i], nextScene);
        }   
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

        Vector2 endDir = new Vector2(curEnd.GetComponent<LevelStartEndPoint>().direction.x, curEnd.GetComponent<LevelStartEndPoint>().direction.z);
        Vector2 startDir = new Vector2(newStart.GetComponent<LevelStartEndPoint>().direction.x, newStart.GetComponent<LevelStartEndPoint>().direction.z);
        Quaternion rotateVector = Quaternion.Euler(0, Utility.AngleBetweenVector2(startDir, endDir),0);
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
