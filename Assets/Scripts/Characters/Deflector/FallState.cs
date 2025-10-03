using System.Collections.Generic;
using UnityEngine;

public class FallState : CharacterAirState
{

    AirStateResource.JumpInfo currentJumpInfo;
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        JumpState jumpState =  (JumpState) fsm.TryGetState<JumpState>();
        if (jumpState != null )
        {
            currentJumpInfo = jumpState.currentJumpInfo;
            Debug.Log("Found jump state, using that jump info ");
        }
    }
    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);

        if (msg != null)
        {
            if (msg.ContainsKey("JumpInfo"))
            {
                currentJumpInfo = (AirStateResource.JumpInfo)msg["JumpInfo"];
            }
        }
       
    }
    public override void PhysicsProcess()
    {
        Vector3 moveSpeed = AirStrafeLogic();
            float fallSpeed = _rb.linearVelocity.y;
        fallSpeed -= currentJumpInfo.fallGravity * Time.fixedDeltaTime;
        fallSpeed = Mathf.Clamp(fallSpeed, currentJumpInfo.maxFallSpeed, currentJumpInfo.jumpVelocity);

        moveSpeed.y = fallSpeed;

        _rb.linearVelocity = moveSpeed;

        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            if (GetMovementDir().magnitude < MOVE_DEADZONE)
            {
                fsm.TransitionTo<IdleState>();
                return;
            }
            else
            {
                fsm.TransitionTo<RunState>();
                return;
            }
        }
    }
}

