using System.Collections.Generic;
using UnityEngine;
public class JumpState : CharacterAirState
{
    public AirStateResource.JumpInfo currentJumpInfo;
    [SerializeField] int airJumps = 1;
    int remainingAirJumps = 1;


    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        currentJumpInfo.InitJumpInfo();
    }
    public override void Enter(Dictionary<string, object> msg)
    {
        base.Enter(msg);
       
        Vector2 newSpeed = _rb.linearVelocity;
        newSpeed.y = currentJumpInfo.jumpVelocity;
        _rb.linearVelocity = newSpeed;
        Debug.Log("Entered jump state with velocity of " + currentJumpInfo.jumpVelocity);
    }
    public override void PhysicsProcess()
    {
        Vector3 moveSpeed = AirStrafeLogic();
        float fallSpeed = _rb.linearVelocity.y;
        fallSpeed -= currentJumpInfo.jumpGravity * Time.fixedDeltaTime;
        fallSpeed = Mathf.Clamp(fallSpeed, currentJumpInfo.maxFallSpeed, currentJumpInfo.jumpVelocity);

        moveSpeed.y = fallSpeed;

        _rb.linearVelocity = moveSpeed;
       
        if (fallSpeed < 0)
        {
            Dictionary<string, object> msg = new()
            {
                ["JumpInfo"] = currentJumpInfo
            };
            fsm.TransitionTo<FallState>(msg);
        }
    }
    

    public override void InactivePhysicsProcess()
    {
        if (IsGrounded())
        {
            remainingAirJumps = airJumps;
        }
    }

    public override Dictionary<string, object> GetStateData()
    {
        Dictionary<string, object> data = new()
        {
            ["AirJumps"] = remainingAirJumps
        };
        return data;
    }
}
