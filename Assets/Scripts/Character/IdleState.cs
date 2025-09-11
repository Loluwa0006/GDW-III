using UnityEngine;

using UnityEngine.InputSystem;

public class IdleState : CharacterBaseState
{
    [SerializeField] float decelRate = 0.85f;

    Rigidbody _rb;

    PlayerInput playerInput;

    float horizAxis = 0.0f;
    float vertAxis = 0.0f;

    

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        playerInput = cha.GetComponent<PlayerInput>();
        _rb = cha.GetComponent<Rigidbody>();
    }

    public override void Process()
    {
        horizAxis = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        vertAxis = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();
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
        Vector2 moveDir = new (vertAxis, horizAxis);
        if (moveDir.magnitude > MOVE_DEADZONE)
        {
            fsm.TransitionTo<RunState>();
            return;
        }
        Vector2 newSpeed = _rb.linearVelocity * decelRate;
        _rb.linearVelocity = newSpeed;
    }
}
