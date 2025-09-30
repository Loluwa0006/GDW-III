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
