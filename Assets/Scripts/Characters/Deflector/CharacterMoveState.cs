using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMoveState : CharacterBaseState
{
    protected const float DAMPING_RATE = 0.97f;

    [SerializeField] protected float moveAcceleration = 2.5f;
    [SerializeField] protected float moveSpeed = 20.0f;

    [SerializeField] BufferHelper jumpBuffer;
    [SerializeField] BufferHelper skillOneBuffer;
    [SerializeField] BufferHelper skillTwoBuffer;


    protected Rigidbody _rb;


    protected Vector3 moveDir = new();
    
    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = cha.GetComponent<Rigidbody>();
        playerInput = cha.GetComponent<PlayerInput>();
    }

    public override void Process()
    {
        moveDir = GetMovementDir();
        if (jumpBuffer.Buffered)
        {
            jumpBuffer.Consume();
            fsm.TransitionTo<JumpState>();
            return;
        }
        if (skillOneBuffer.Buffered)
        {
            skillOneBuffer.Consume();
            fsm.TransitionToSkill(1);
            
        }
        else if (skillTwoBuffer.Buffered)
        {
            skillTwoBuffer.Consume();
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
        if (moveDir.magnitude <= MOVE_DEADZONE)
        {
            fsm.TransitionTo<IdleState>();
            return;
        }
        Vector3 newSpeed = moveDir.normalized * moveAcceleration;
     
            character.velocityManager.AddInternalVelocity(newSpeed);
            character.velocityManager.ClampInternalVelocity(moveSpeed);
        

    }
}
