using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAirState : CharacterBaseState
{

    [SerializeField] protected AirStateResource airStateHelper;
    protected Rigidbody _rb;
    protected PlayerInput playerInput;

    protected AirStateResource.JumpInfo currentJumpInfo;

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        airStateHelper.InitializeResource();
        _rb = cha.GetComponent<Rigidbody>();
        playerInput = cha.GetComponent<PlayerInput>();
    }



  

    
}
