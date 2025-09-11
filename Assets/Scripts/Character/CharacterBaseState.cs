using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterBaseState : MonoBehaviour
{
    protected const float MOVE_DEADZONE = 0.1f;

    public bool hasInactiveProcess = false;
    public bool hasInactivePhysicsProcess = false;
    protected CharacterStateMachine fsm;
    protected BaseCharacter character;
    float BOXCAST_RATIO = 0.05f;
    protected LayerMask groundMask;

    protected BoxCollider _rbCollider;

    public virtual void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        fsm = s_machine;
        character = cha;
        groundMask = LayerMask.GetMask("Ground");
        _rbCollider = cha.GetComponent<BoxCollider>();
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

}
