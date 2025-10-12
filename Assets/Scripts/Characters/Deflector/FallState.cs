using System.Collections.Generic;
using UnityEngine;

public class FallState : CharacterAirState
{

    AirStateResource.JumpInfo currentJumpInfo;
    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        JumpState jumpState =  (JumpState) fsm.TryGetState<JumpState>();
        if (jumpState != null )
        {
            currentJumpInfo = jumpState.currentJumpInfo;
            Debug.Log("Found jump state, using that jump info ");
        }
    }

    public override void PhysicsProcess()
    {
        base.PhysicsProcess();

        if (IsGrounded())
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

    protected override float GetGravity()
    {
        return currentJumpInfo.fallGravity;
    }

    protected override AirStateResource.JumpInfo GetJumpInfo()
    {
        return currentJumpInfo;
    }
}

