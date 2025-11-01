using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBaseState : MonoBehaviour
{
    protected const float MOVE_DEADZONE = 0.1f;

    [HideInInspector] public BaseSpeaker character;
    public bool hasInactiveProcess = false;
    public bool hasInactivePhysicsProcess = false;
    public bool deflectAllowed = true;

    protected CharacterStateMachine fsm;
    protected LayerMask groundMask;

    protected BoxCollider _rbCollider;

    protected PlayerInput playerInput;

    float BOXCAST_RATIO = 0.85f;


    public virtual void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        fsm = s_machine;
        character = cha;
        groundMask = LayerMask.GetMask("Ground");
        _rbCollider = cha.GetComponent<BoxCollider>();
        playerInput = cha.GetComponent<PlayerInput>();
    }
    public virtual void Enter(Dictionary<string, object> msg = null)
    {

    }

    public virtual void Exit()
    {

    }

    public virtual void Process()
    {

    }

    public virtual void PhysicsProcess()
    {

    }

    public virtual void InactiveProcess()
    {

    }

    public virtual void InactivePhysicsProcess()
    {

    }

    public bool IsGrounded()
    {
        float castDistance = (_rbCollider.size.y / 2.0f) + 0.05f;
        bool hit = Physics.BoxCast
            (
            _rbCollider.bounds.center,
            _rbCollider.size / 2.0f * BOXCAST_RATIO,
            Vector3.down,
            character.transform.rotation,
            castDistance,
            groundMask
            );
        return hit;
    }

    protected Vector3 GetMovementDir()
    {
        float x = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        float z = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();
        Vector3 moveDir = new Vector3(x, 0, z).normalized;
        return moveDir;
    }

    public virtual Dictionary<string, object> GetStateData()
    {
        return new Dictionary<string, object>();
    }

}
