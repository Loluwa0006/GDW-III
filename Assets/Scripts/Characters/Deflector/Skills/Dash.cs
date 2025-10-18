using System.Collections.Generic;
using UnityEngine;

public class Dash : BaseSkill
{

    const float DASH_DEADZONE_REQUIREMENT = 0.2f;

    [SerializeField] int dashDistance = 10;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField, Range(0, 1)] float speedMaintained;
    [SerializeField] ParticleSystem dashParticles;
    [SerializeField] DamageInfo damageInfo;

    float dashSpeed;
    float dashTracker;

    Vector3 dashDir = Vector3.zero;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        dashSpeed = dashDistance / dashDuration;
        SetDashParticleEmission(false);
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        dashDir = GetMovementDir().normalized;
        dashTracker = 0;
        base.OnSkillUsed();
        character.velocityManager.OverwriteInternalSpeed(dashDir * dashSpeed);
        SetDashParticleEmission(true);
    }

    void SetDashParticleEmission(bool value)
    {
        var emission = dashParticles.emission;
        emission.enabled = value;
    }

    public override void PhysicsProcess()
    {
        dashTracker += Time.fixedDeltaTime;
        if (dashTracker >= dashDuration)
        {
            character.velocityManager.OverwriteInternalSpeed(dashSpeed * speedMaintained * dashDir);
            if (!IsGrounded())
            {
                fsm.TransitionTo<FallState>();
            }
            else
            {
                var moveDir = GetMovementDir();

                if (moveDir.magnitude > MOVE_DEADZONE)
                {
                    fsm.TransitionTo<RunState>();
                }
                else
                {
                    fsm.TransitionTo<IdleState>();
                }
            }

        }

        dashParticles.transform.rotation = Quaternion.Euler(dashDir + new Vector3(-180, 0, 0));
    }

    public override void Process()
    {
        if (oppositeSkillBuffer.Buffered)
        {
            oppositeSkillBuffer.Consume();
            fsm.TransitionToSkill(oppositeSkillIndex);
        }
    }

    public override void Exit()
    {
        SetDashParticleEmission(false);
    }

    public override bool SkillAvailable()
    {
        bool hasStamina = staminaComponent.GetStamina() > staminaCost || staminaComponent.HasForesight();
        return hasStamina && GetMovementDir().magnitude > DASH_DEADZONE_REQUIREMENT;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BaseSpeaker speaker))
        {
            speaker.healthComponent.Damage(damageInfo);
        }
    }
}
