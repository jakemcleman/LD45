using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private bool pauseOpen;

    private CanvasGroup pausePanel;
    private CanvasGroup curMenu;
    private CanvasGroup hud;

    FMOD.Studio.VCA vca_mus;
    FMOD.Studio.VCA vca_sfx;

    public static bool Paused
    {
        get
        {
            MenuController mc = FindObjectOfType<MenuController>();
            return mc != null ? mc.GetPauseOpen() : false;
        }
    }

    public bool GetPauseOpen()
    {
        return pauseOpen;
    }

    // Start is called before the first frame update
    void Start()
    {
        vca_mus = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
        vca_sfx = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");
        vca_mus.setVolume(1.0f);
        vca_sfx.setVolume(1.0f);

        GameObject pausePanelObj = GameObject.Find("PauseMenu");
        if(pausePanelObj != null) pausePanel = pausePanelObj.GetComponent<CanvasGroup>();

        GameObject hudObj = GameObject.Find("HUD");
        if(hudObj != null)hud = hudObj.GetComponent<CanvasGroup>();

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

    public void LoadMainMenu()
    {
        SceneLoader.gameMusicHasStarted = false;

        Time.timeScale = 1;
        pauseOpen = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameStart()
    {
        SceneManager.LoadScene("Level0");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void MusicVolume (float val)
    {
        vca_mus.setVolume(val);
    }

    public void SFXVolume(float val)
    {
        vca_sfx.setVolume(val);
    }
}
