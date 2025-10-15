//using System.Collections;
//using System.Collections.Generic;
//#if Unity_Editor
//using UnityEditor;
//#endif
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class MainMenuController : MonoBehaviour
//{
//    public GameObject MainMenuUI;
//    public GameObject OptionsMenu;

//    public void Start()
//    {
//        Time.timeScale = 0f;
//    }
//    public void PlayGame()
//    {
//        SceneManager.LoadScene("SampleScene");
//        Time.timeScale = 1f;
//    }

//    public void Options()
//    {
//        OptionsMenuController.PreviousScene = "MainMenu";
//        OptionsMenu.SetActive(true);
//        MainMenuUI.SetActive(false);
//    }
    
//    public void QuitGame()
//    {
//        Debug.Log("Quit");

//#if Unity_Editor
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
