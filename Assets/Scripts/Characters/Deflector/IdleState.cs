using UnityEngine;

using UnityEngine.InputSystem;

public class IdleState : CharacterMoveState
{
    [SerializeField] float decelRate = 0.85f;


    public override void PhysicsProcess()
    {
        if (!IsGrounded())
        {
            fsm.TransitionTo<FallState>();
            return;
        }
        if (moveDir.magnitude > MOVE_DEADZONE)
        {
            fsm.TransitionTo<RunState>();
            return;
        }
        Vector2 newSpeed = _rb.linearVelocity * decelRate;
        character.velocityManager.OverwriteInternalSpeed(newSpeed);
    }
}
