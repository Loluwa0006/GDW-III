using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  [SerializeField]  List<TransitionButton> transitionButtons = new();

    private void Awake()
    {
        foreach (var transition in transitionButtons)
        {
            if (transition.button != null)
            {
                string sceneName = transition.value.ToString();
                transition.button.onClick.AddListener(() => TransitionToScene(sceneName));
            }
        }
    }


    public void TransitionToScene(string newScene)
    {
        if (Application.CanStreamedLevelBeLoaded(newScene))
        {
            SceneManager.LoadScene(newScene);
        }
        else
        {
            Debug.Log("Can't transition to scene " + newScene);
        }
    }

    public void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}


