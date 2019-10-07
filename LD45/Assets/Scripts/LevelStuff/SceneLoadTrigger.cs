﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour
{
    private SceneLoader sceneLoader;

    public string sceneName = null;

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
