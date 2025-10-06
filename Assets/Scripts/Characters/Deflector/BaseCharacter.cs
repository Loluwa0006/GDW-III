using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

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

    [HideInInspector] public int teamIndex;
    
    bool init = false;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();  
        }
    }


    public void InitPlayer(MatchData.PlayerInfo info, int index)
    {

        teamIndex = index;
        playerInput.SwitchCurrentControlScheme(info.device);
        if (info.keyboardPlayerTwo)
        {
            playerInput.SwitchCurrentActionMap("CombatKeyboardTwo");
        }

     
         playerModel.material = playerColors[index - 1];
        
        characterStateMachine.CreateSkills(info);
        characterStateMachine.InitMachine();
        init = true;
    }

    private void Update()
    {
        if (GameManager.inSpecialStop || !init) { return; }
        characterStateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        if (GameManager.inSpecialStop || !init) { return; }
        characterStateMachine.FixedUpdateState();
    }

}
