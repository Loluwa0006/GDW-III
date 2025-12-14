using TMPro;
using UnityEngine;

public class StateTracker : MonoBehaviour
{
    [SerializeField] CharacterStateMachine stateMachine;

    [SerializeField] TMP_Text display;


    private void Update()
    {
        if (stateMachine == null || display == null) return;
        display.text = "State: " + stateMachine.currentState.name;
    }
}
