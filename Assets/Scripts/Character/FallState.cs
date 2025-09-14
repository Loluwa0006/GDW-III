using System.Collections.Generic;
using UnityEngine;

public class FallState : CharacterAirState
{
    protected AirStateResource.JumpTypes jumpType;

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
        if (currentJumpInfo == null || msg == null )
        {
            currentJumpInfo = airStateHelper.jumpMap[AirStateResource.JumpTypes.GroundJump]; //assume ground jump gravity as fallback
        }
    }
    public override void PhysicsProcess()
    {
        Vector3 moveDir = GetMovementDir();
        Vector3 moveSpeed = new();

        Vector3 strafeSpeed = new Vector3(moveDir.x, 0, moveDir.z).normalized * airStateHelper.airAcceleration;
        moveSpeed.x = strafeSpeed.x + _rb.linearVelocity.x;
        moveSpeed.z = strafeSpeed.z + _rb.linearVelocity.z;
        moveSpeed = Vector3.ClampMagnitude(moveSpeed, airStateHelper.airStrafeSpeed);

        float fallSpeed = _rb.linearVelocity.y;
        fallSpeed -= currentJumpInfo.fallGravity * Time.deltaTime;
        fallSpeed = Mathf.Clamp(fallSpeed, currentJumpInfo.maxFallSpeed, currentJumpInfo.jumpVelocity);

        moveSpeed.y = fallSpeed;

        _rb.linearVelocity = moveSpeed;

        bool isGrounded = IsGrounded();
        Debug.Log("Grounded = " + isGrounded);

        if (isGrounded)
        {
            if (moveDir.magnitude < MOVE_DEADZONE)
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

