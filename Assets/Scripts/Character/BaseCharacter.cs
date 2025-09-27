using UnityEngine;
using UnityEngine.InputSystem;

public class BaseCharacter : MonoBehaviour
{

    public CharacterStateMachine characterStateMachine;
    
    public HealthComponent healthComponent;
    public DeflectManager deflectManager;
    public StaminaComponent staminaComponent;


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
