using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    private bool pauseOpen;

    private CanvasGroup pausePanel;
    private CanvasGroup curMenu;
    private CanvasGroup hud;

    public bool GetPauseOpen()
    {
        return pauseOpen;
    }

    // Start is called before the first frame update
    void Start()
    {
        pausePanel = GameObject.Find("PauseMenu").GetComponent<CanvasGroup>();
        hud = GameObject.Find("HUD").GetComponent<CanvasGroup>();

        pauseOpen = false;

        curMenu = hud;
    }

    public void TogglePause()
    {
        if (!GetPauseOpen()) OpenPause();
        else ClosePause();
    }

    private void SwitchMenu(CanvasGroup newMenu)
    {
        curMenu.alpha = 0;
        curMenu.interactable = false;
        curMenu.blocksRaycasts = false;
        newMenu.alpha = 1;
        newMenu.interactable = true;
        newMenu.blocksRaycasts = true;
        curMenu = newMenu;
    }

    public void OpenPause()
    {
        SwitchMenu(pausePanel);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        pauseOpen = true;
    }

    public void ClosePause()
    {
        SwitchMenu(hud);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        pauseOpen = false;
    }

    public void MusicVolume (float val)
    {
        //Put music volume shit here
    }

    public void SFXVolume(float val)
    {
        //Put SFX volume shit here
    }
}
