using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour
{
    private SceneLoader sceneLoader;

    public enum Action
    {
        Load,
        Unload,
        Both
    }

    public Action action;

    // Start is called before the first frame update
    void Start()
    {
        sceneLoader = GameObject.Find("Map").GetComponent<SceneLoader>();
        if (sceneLoader = null)
        {
            Debug.LogError("No Map in Scene, please add a map with a scene loader");
        }
        else
        {
            Debug.Log(sceneLoader);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            switch (action)
            {
                case Action.Load:
                    sceneLoader.StartLoad(false);
                    break;

                case Action.Unload:
                    sceneLoader.UnLoadScene();
                    break;

                case Action.Both:
                    sceneLoader.StartLoad(true);
                    break;
            }
        }
    }
}
