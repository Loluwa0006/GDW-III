using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMoveState : CharacterBaseState
{
    protected const float DAMPING_RATE = 0.97f;

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
        if (playerInput.actions["SkillOne"].WasPerformedThisFrame())
        {
            Debug.Log("Skill one pressed");
            fsm.TransitionToSkill(1);
        }
        else if (playerInput.actions["SkillTwo"].WasPerformedThisFrame())
        {
            Debug.Log("Skill two pressed");
            fsm.TransitionToSkill(2);
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
        if (_rb.linearVelocity.magnitude < moveSpeed)
        {
            _rb.linearVelocity += newSpeed;
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, moveSpeed);
        }
        else
        {
            _rb.linearVelocity = _rb.linearVelocity * DAMPING_RATE;
        }
    }
}
