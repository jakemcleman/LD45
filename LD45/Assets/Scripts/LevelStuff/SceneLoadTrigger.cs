using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    private SceneLoader sceneLoader;

    public string sceneName = null;

    public GameObject SceneManagerGO;

    public enum STAction
    {
        Load,
        Unload,
        Both
    }

    public STAction action;

    // Start is called before the first frame update
    void Start()
    {
        sceneLoader = GameObject.FindObjectOfType<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("No Scene Manager found, adding a new one :)");
            Instantiate(SceneManagerGO);
            sceneLoader = SceneManagerGO.GetComponent<SceneLoader>();
            sceneLoader.firstLevelIndex = this.gameObject.scene.buildIndex;
            sceneLoader.ResetCurIndex();
            Debug.Log("Current scene build index: " + this.gameObject.scene.buildIndex);
        }
        else
        {
            //Debug.Log(sceneLoader);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (sceneLoader == null)
        {
            sceneLoader = GameObject.FindObjectOfType<SceneLoader>();
        }

        if (col.tag == "Player")
        {
            switch (action)
            {
                case STAction.Load:
                    sceneLoader.StartLoad(this.gameObject, false);
                    break;

                case STAction.Unload:
                    sceneLoader.UnLoadScene(this.gameObject);
                    break;

                case STAction.Both:
                    sceneLoader.StartLoad(this.gameObject, true);
                    break;
            }
        }
    }
}
