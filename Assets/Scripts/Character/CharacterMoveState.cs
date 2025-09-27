using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMoveState : CharacterBaseState
{
    [SerializeField] float moveAcceleration = 2.5f;
    [SerializeField] float moveSpeed = 20.0f;

    Rigidbody _rb;


    Vector3 moveDir = new();
    
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = cha.GetComponent<Rigidbody>();
        playerInput = cha.GetComponent<PlayerInput>();
    }

    public override void Process()
    {
        moveDir = GetMovementDir();
        if (playerInput.actions["Jump"].WasPerformedThisFrame())
        {
            Debug.Log("Jump pressed");
            fsm.TransitionTo<JumpState>();
            return;
        }
    }

    public override void PhysicsProcess()
    {
        if (moveDir.magnitude < MOVE_DEADZONE)
        {
            fsm.TransitionTo<IdleState>();
            return;
        }
        Debug.Log("Move dir is " + moveDir);
        Vector3 newSpeed = moveDir.normalized * moveAcceleration;
        _rb.linearVelocity += newSpeed;
        _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, moveSpeed);
    }
}
