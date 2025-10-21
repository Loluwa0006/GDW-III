using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GetHitState : CharacterBaseState
{
    const float MAX_FALL_SPEED = 15.0f;

    [SerializeField] BufferHelper jumpBuffer;

    [SerializeField] int additionalIFramesPostEchoHit = 5;

    DamageInfo hitInfo;

    int hitstunTracker = 0;

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        bool hasData = false;
        if (msg != null)
        {
            if (msg.TryGetValue("Data", out object data))
            {
                hitInfo = (DamageInfo)data;
                if (hitInfo.hitstunGravity == DamageInfo.USE_DEFAULT_HITSTUN_GRAVITY) { hitInfo.hitstunGravity = DamageInfo.DEFAULT_HITSTUN_GRAVITY; }
                hasData = true;
                hitstunTracker = hitInfo.hitstun;
            }
        } 
        if (!hasData)
        {
            Debug.LogError( character.name + " entered get-hit state without data");
            ExitHitstunState();
            return;
        }
        Vector3 currentSpeed = character.velocityManager.GetInternalSpeed();
        currentSpeed.y = hitInfo.knockbackLaunch;
        Vector3 knockbackVector = new Vector3(hitInfo.knockbackDir.x, currentSpeed.y, hitInfo.knockbackDir.z).normalized * hitInfo.knockbackDistance;
        character.velocityManager.OverwriteInternalSpeed(knockbackVector);
        if (hitInfo.leaveTargetInvincible)
        {
            Debug.Log("Adding new invuln source to char " + character.name);
            InvulnerabilityEffect invulnEffect = new InvulnerabilityEffect(DamageSource.Ball, hitInfo.hitstun + additionalIFramesPostEchoHit);
            character.healthComponent.AddStatusEffect(invulnEffect, "Invuln" + hitInfo.damageSource.ToString());
        }

    }

   
    void ExitHitstunState()
    {
        Vector3 currentSpeed = character.velocityManager.GetInternalSpeed();
        if (currentSpeed.y > MAX_FALL_SPEED)
        {
            currentSpeed.y = MAX_FALL_SPEED;
        }
        character.velocityManager.OverwriteInternalSpeed(currentSpeed);
        if (jumpBuffer.Buffered)
        {
            if (IsGrounded())
            {
                jumpBuffer.Consume();
                fsm.TransitionTo<JumpState>();
                return;
            }
        }  
        else
        {
            fsm.TransitionTo<FallState>();
        }
    }

    public override void PhysicsProcess()
    {
        character.velocityManager.AddInternalVelocity(new Vector3(0, -hitInfo.hitstunGravity, 0));
        Vector3 currentSpeed = character.velocityManager.GetInternalSpeed();
        if (currentSpeed.y < - MAX_FALL_SPEED)
        {
            currentSpeed.y = -MAX_FALL_SPEED;
            character.velocityManager.OverwriteInternalSpeed(currentSpeed);
        }

        HitstunLogic();

    }

    void HitstunLogic()
    {
        if (GameManager.inSpecialStop) { return; }
        hitstunTracker -= 1;
        if (hitstunTracker <= 0)
        {
            ExitHitstunState();
        }

    }
}
