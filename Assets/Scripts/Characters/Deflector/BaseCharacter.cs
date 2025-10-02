using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BaseCharacter : MonoBehaviour
{

    public CharacterStateMachine characterStateMachine;
    
    public HealthComponent healthComponent;
    public DeflectManager deflectManager;
    public StaminaComponent staminaComponent;
    public PlayerInput playerInput;
    public GameObject playerModel;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();  
        }
    }


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
