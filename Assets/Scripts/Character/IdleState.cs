using UnityEngine;

using UnityEngine.InputSystem;

public class IdleState : CharacterBaseState
{
    [SerializeField] float decelRate = 0.85f;

    Rigidbody _rb;
    Vector3 moveDir;
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        playerInput = cha.GetComponent<PlayerInput>();
        _rb = cha.GetComponent<Rigidbody>();
    }

    public override void Process()
    {
        moveDir = GetMovementDir();

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            Debug.Log("Jump pressed");
            fsm.TransitionTo<JumpState>();
            return;
        }
    }

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
        _rb.linearVelocity = newSpeed;
    }
}
