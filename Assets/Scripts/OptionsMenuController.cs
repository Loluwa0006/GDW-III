using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuController : MonoBehaviour
{
    public static string PreviousScene;

    public PauseMenuController PauseScript;

    public MainMenuController MainScript;


    public void ReturnToPreviousScene()
    {
        if (PreviousScene == null)
        {
            PreviousScene = "MainMenu";
        }
        if (PreviousScene == "SampleScene")
        {
            PauseScript.pauseMenuUI.SetActive(true);
            PauseScript.OptionsMenu.SetActive(false);
        }
        if (PreviousScene == "MainMenu")
        {
            MainScript.MainMenuUI.SetActive(true);
            MainScript.OptionsMenu.SetActive(false);
        }
    }
}
