using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMoveState : CharacterBaseState
{
    [SerializeField] float moveAcceleration = 2.5f;
    [SerializeField] float moveSpeed = 20.0f;

    Rigidbody _rb;
    PlayerInput playerInput;

    float horizAxis = 0.0f;
    float vertAxis = 0.0f;

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = cha.GetComponent<Rigidbody>();
        playerInput = cha.GetComponent<PlayerInput>();
    }

    public override void Process()
    {
        horizAxis = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        vertAxis = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();
        if (playerInput.actions["Jump"].WasPerformedThisFrame())
        {
            Debug.Log("Jump pressed");
            fsm.TransitionTo<JumpState>();
            return;
        }
    }

    public override void PhysicsProcess()
    {
        Vector2 moveDir = new (horizAxis, vertAxis);
       
        if (moveDir.magnitude < MOVE_DEADZONE)
        {
            fsm.TransitionTo<IdleState>();
            return;
        }
        _rb.linearVelocity = _rb.linearVelocity + new Vector3(moveDir.x, 0, moveDir.y).normalized * moveAcceleration;
        _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, moveSpeed);
    }
}
