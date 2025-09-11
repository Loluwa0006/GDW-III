using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class JumpState : CharacterAirState
{

    [SerializeField] int airJumps = 1;
    int remainingAirJumps = 1;
    [SerializeField] protected AirStateResource.JumpTypes jumpType;

    public override void Enter(Dictionary<string, object> msg)
    {
        base.Enter(msg);
        currentJumpInfo = airStateHelper.jumpMap[jumpType];
        Vector2 newSpeed = _rb.linearVelocity;
        newSpeed.y = currentJumpInfo.jumpVelocity;
        _rb.linearVelocity = newSpeed;
        Debug.Log("Entered jump state with velocity of " + currentJumpInfo.jumpVelocity);
    }
    public override void PhysicsProcess()
    {
        Vector3 moveDir = new();
        Vector3 moveSpeed = new();
        moveDir.x = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        moveDir.z = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();

        Vector3 strafeSpeed = new Vector3(moveDir.x, 0, moveDir.z).normalized * airStateHelper.airAcceleration;
        moveSpeed.x = strafeSpeed.x + _rb.linearVelocity.x;
        moveSpeed.z = strafeSpeed.z + _rb.linearVelocity.z;
        moveSpeed = Vector3.ClampMagnitude(moveSpeed, airStateHelper.airStrafeSpeed);

        float fallSpeed = _rb.linearVelocity.y;
        fallSpeed -= currentJumpInfo.jumpGravity * Time.deltaTime;
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
}
