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
    public MeshRenderer playerModel;
    public VelocityManager velocityManager;

    public List<Material> playerColors = new();

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
        if (GameManager.inSpecialStop) { return; }
        characterStateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        if (GameManager.inSpecialStop) { return; }
        characterStateMachine.FixedUpdateState();
    }

}
