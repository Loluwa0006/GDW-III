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

    float BOXCAST_RATIO = 0.05f;


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
        bool hit = Physics.BoxCast
            (
            character.transform.position,
            character.transform.localScale * BOXCAST_RATIO,
            Vector3.down,
            character.transform.rotation,
            _rbCollider.size.y * 1.01f,
            groundMask

            );
        return hit;
    }

    protected Vector3 GetMovementDir()
    {


          float x = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
          float z = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>(); 
        
            return new Vector3(x, 0, z).normalized;

    }

    public virtual Dictionary<string, object> GetStateData()
    {
        return new Dictionary<string, object>();
    }

}
