using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dash : BaseSkill
{

    const float DASH_DEADZONE_REQUIREMENT = 0.2f;

    [SerializeField] int dashDistance = 10;
    [SerializeField] float dashDuration = 0.2f;

    float dashSpeed;
    float dashTracker;

    Vector3 dashDir = Vector3.zero;

    Rigidbody rb;


    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        dashSpeed = dashDistance / dashDuration;
        rb = character.gameObject.GetComponent<Rigidbody>();
        deflectAllowed = false;
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        Debug.Log("Using dash");
        dashDir = GetMovementDir().normalized;

        character.deflectManager.stateAllowsDeflect = false;

        dashTracker = 0;
        base.OnSkillUsed();
    }

    public override void PhysicsProcess()
    {
        dashTracker += Time.deltaTime;
        rb.linearVelocity = dashDir * dashSpeed;
        if (dashTracker >= dashDuration)
        {
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


    public override bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost && GetMovementDir().magnitude > DASH_DEADZONE_REQUIREMENT;
    }
}
