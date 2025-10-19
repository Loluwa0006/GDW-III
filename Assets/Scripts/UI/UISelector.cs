using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UISelector : MonoBehaviour
{

    [SerializeField] PlayerInput pInput;
    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;
    [SerializeField] Image image;
    [SerializeField] TMP_Text indexDisplay;

    public TMP_Text skillOneDisplay;
    public TMP_Text skillTwoDisplay;
    public RectTransform rectTransform;

    [HideInInspector] public int teamIndex = 0;
    [HideInInspector] public UnityEvent<UISelector> selectorLocked = new();

    PreGameSelectionManager manager;

    [HideInInspector] public bool locked = false;


    public void Init(PreGameSelectionManager manager, int index)
    {
        if (pInput == null)
        {
            pInput = GetComponent<PlayerInput>();
        }
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        this.manager = manager;
        indexDisplay.text = index.ToString();
        skillOneDisplay.gameObject.SetActive(false);
        skillTwoDisplay.gameObject.SetActive(false);
    }






    private void Update()
    {
        if (manager == null) { return; }
        switch (manager.selectionScreen)
        {
        
            case SelectionScreen.TeamSelect:
                if (pInput.actions["Left"].WasPerformedThisFrame())
                {
                    manager.OnSelectionMoved(this, -1);
                }
                else if (pInput.actions["Right"].WasPerformedThisFrame())
                {
                    manager.OnSelectionMoved(this, 1);
                }
                    break;
            case SelectionScreen.SkillSelect:
                if (pInput.actions["SkillOne"].WasPerformedThisFrame())
                {
                    manager.OnSkillPressed(this, 1);
                }
                else if (pInput.actions["SkillTwo"].WasPerformedThisFrame())
                {
                    manager.OnSkillPressed(this, 2);
                }
                break;
    }
           if (pInput.actions["Deflect"].WasPerformedThisFrame())
        {
            if (locked)
            {
                ResetSelection();
            }
            else
            {
                LockSelection();
            }
        }
           if (pInput.actions["Jump"].WasPerformedThisFrame())
        {
            manager.ReturnToPreviousScreen();
        }
    }

    public void LockSelection()
    {
        locked = true;
        image.color = selectedColor;
        selectorLocked.Invoke(this);
     //   Debug.Log("Locking selector " + name);
    }

    public void ResetSelection()
    {
        locked = false;
        image.color = unselectedColor;
    //    Debug.Log("Unlocking YIPPIE selector " + name);
    }

  
}


