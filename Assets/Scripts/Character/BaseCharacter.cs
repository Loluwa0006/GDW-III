using UnityEngine;
using UnityEngine.InputSystem;

public class BaseCharacter : MonoBehaviour
{

    [SerializeField] CharacterStateMachine characterStateMachine;



    private void Start()
    {
        characterStateMachine.InitMachine();
    }

    private void Update()
    {
        characterStateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        characterStateMachine.FixedUpdateState();
    }

}
