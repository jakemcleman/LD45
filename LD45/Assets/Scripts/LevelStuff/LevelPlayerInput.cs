using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayerInput : MonoBehaviour
{
    private PlayerHealth pHealth;
    private MenuController mc;

    // Start is called before the first frame update
    void Start()
    {
        pHealth = GetComponent<PlayerHealth>();
        mc = GameObject.FindObjectOfType<MenuController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Reset");
            pHealth.Respawn();
        }

        if (Input.GetButtonDown("Cancel"))
        {
            Debug.Log("Esc Recieved");
            mc.TogglePause();
        }
    }
}
