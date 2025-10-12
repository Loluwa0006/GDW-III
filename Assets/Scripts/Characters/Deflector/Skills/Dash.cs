using System.Collections.Generic;
using UnityEngine;

public class Dash : BaseSkill
{

    const float DASH_DEADZONE_REQUIREMENT = 0.2f;

    [SerializeField] int dashDistance = 10;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField, Range (0,1)] float speedMaintained;

    float dashSpeed;
    float dashTracker;

    Vector3 dashDir = Vector3.zero;

    Rigidbody rb;


    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        dashSpeed = dashDistance / dashDuration;
        rb = character.gameObject.GetComponent<Rigidbody>();
        deflectAllowed = false;
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        dashDir = GetMovementDir().normalized;
        dashTracker = 0;
        base.OnSkillUsed();
        character.velocityManager.OverwriteInternalSpeed(dashDir * dashSpeed);
    }

    
    public override void PhysicsProcess()
    {
        dashTracker += Time.fixedDeltaTime;
        if (dashTracker >= dashDuration)
        {

            character.velocityManager.OverwriteInternalSpeed((dashDir * dashSpeed) * speedMaintained);
            if (!IsGrounded())
            {
                fsm.TransitionTo<FallState>();
            }
            else
            {
                var moveDir = GetMovementDir();

                if (moveDir.magnitude > 0.1f)
                {
                    fsm.TransitionTo<RunState>();
                }
                else
                {
                    fsm.TransitionTo<IdleState>();
                }
            }

        }
    }

    private void Update()
    {
        if (oppositeSkillBuffer.Buffered)
        {
            oppositeSkillBuffer.Consume(); 
            fsm.TransitionToSkill(oppositeSkillIndex);
        }
    }


    public override bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost && GetMovementDir().magnitude > DASH_DEADZONE_REQUIREMENT;
    }
}
