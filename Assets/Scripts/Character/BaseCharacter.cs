using UnityEngine;
using UnityEngine.InputSystem;

public class BaseCharacter : MonoBehaviour
{

    [SerializeField] CharacterStateMachine characterStateMachine;
    
    public HealthComponent healthComponent;
    public DeflectManager deflectManager;


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
