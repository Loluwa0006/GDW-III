using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Takeback : BaseSkill
{
    enum TakebackState
    {
        Catching,
        Holding,
        Throwing,
        Whiff,
        None
    }

    TakebackState currentState = TakebackState.Catching;

    [SerializeField] float whiffDuration = 0.9f;
    [SerializeField] float yoyoDuration = 2.7f; //amount of time after throwing you have to yo-yo
    [SerializeField] int yoyoCost = 30;
    [SerializeField] int holdStaminaDrainRate = 8;
    [SerializeField] float decelRate = 0.9f;
    [SerializeField] float tacklePushback = 8.0f;


    [SerializeField] Transform ballHolder;
    [Header("Particles")]
    [SerializeField] ParticleSystem catchParticle;
    [SerializeField] ParticleSystem throwParticle;
    float catchDuration;

    float durationTracker = 0.0f;
    float whiffTracker = 0.0f;
    float yoyoTracker = 0.0f;
    int holdTracker;

    float previousEchoSpeed = 0.0f;

    BaseEcho heldBall;
    BaseSpeaker enemySpeaker;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        catchDuration = cha.deflectManager.GetGoodDeflectDuration();
        StartCoroutine(FindOppositeSpeaker());
        throwParticle.transform.SetParent(null);
    }

    IEnumerator FindOppositeSpeaker()
    {
        yield return new WaitForFixedUpdate();
        var speakers = FindObjectsByType<BaseSpeaker>(FindObjectsSortMode.None);
        foreach (var speaker in speakers)
        {
            if (speaker == character) { continue; }
            Debug.Log( character.name + " is looking at char " + speaker.name);
            enemySpeaker = speaker;
            break;
        }
    }


    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        StartCatch();
        if (!staminaComponent.HasForesight())
        {
            staminaComponent.DamageStamina(staminaCost, 0, false);
        }
        currentState = TakebackState.Catching;
        durationTracker = catchDuration;
        skillBuffer.Consume();
    }


    public override void Process()
    {

        switch (currentState)
        {
            case TakebackState.Catching:
                durationTracker -= Time.deltaTime;
                if (durationTracker <= 0.0f)
                {
                    whiffTracker = whiffDuration;
                    currentState = TakebackState.Whiff;
                }
                break;
            case TakebackState.Whiff:
                whiffTracker -= Time.deltaTime;
                if (whiffTracker <= 0.0f)
                {
                    ExitState();
                }
                break;

        }

        if (oppositeSkillBuffer != null)
        {
            if (oppositeSkillBuffer.Buffered)
            {
                fsm.TransitionToSkill(oppositeSkillIndex);
                return;
            }
        }
        
    }

    public override void InactivePhysicsProcess()
    {
        switch (currentState)
        {
            case TakebackState.Holding:
                holdTracker += 1;
                if (holdTracker == holdStaminaDrainRate)
                {
                    if (!staminaComponent.HasForesight())
                    {
                        staminaComponent.DamageStamina(1, 0, false);
                        if (staminaComponent.GetStamina() < staminaCost)
                        {
                            DropBall();
                        }
                    }
                    holdTracker = 0;
                }
                break;
        }
      
    }

    public override void InactiveProcess()
    {
        if (!skillAction.IsPressed() && currentState == TakebackState.Holding && heldBall != null)
        {
            ThrowBall();
        }

        if (currentState == TakebackState.Throwing && skillAction.WasPerformedThisFrame() && yoyoTracker > 0)
        {
            YoYoBall();
        }
        if (character.velocityManager.GetExternalSpeed("TakebackTackle") != VelocityManager.MISSING_VELOCITY_VALUE)
        {
            Vector3 currentSpeed = character.velocityManager.GetExternalSpeed("TakebackTackle") * decelRate;
            if (currentSpeed.magnitude <=  0.001f)
            {
                character.velocityManager.RemoveExternalSpeedSource("TakebackTackle");
            }
            else
            {
                character.velocityManager.OverwriteExternalSpeed("TakebackTackle", currentSpeed);
            }
        }
    }

    public override void PhysicsProcess()
    {
        var speed = character.velocityManager.GetInternalSpeed();
        speed *= decelRate;
        character.velocityManager.OverwriteInternalSpeed(speed);


    }


    public override bool OnCharacterHit(DamageInfo info)
    {
        if (currentState == TakebackState.Catching && info.damageSource == DamageSource.Ball) 
        {
            EnterHoldState(info);
            return false;
        }
        return base.OnCharacterHit(info);
    }

    void StartCatch()
    {
        character.healthComponent.AddStatusEffect(new InvulnerabilityEffect(DamageSource.Ball, Mathf.CeilToInt(catchDuration / Time.fixedDeltaTime), false), "TakebackCatch");
        currentState = TakebackState.Catching;
        durationTracker = catchDuration;
    }

    void EnterHoldState(DamageInfo info)
    {
        if (!info.attacker.TryGetComponent(out BaseEcho echo))
        {
            Debug.LogWarning("Idk why " + echo.transform.name + "is labeled as a ball, it doesn't have the component ");
            return;
        }
        heldBall = echo;
       
        previousEchoSpeed = echo.GetSpeed();
        echo.SuspendProjectile(false, true);
        currentState = TakebackState.Holding;
        echo.transform.parent = ballHolder.transform;
        echo.transform.localPosition = Vector3.zero;

        if (catchParticle != null)
        {
            catchParticle.transform.position = character.transform.position;
            catchParticle.Play();
        }
        character.SetLookTarget(enemySpeaker.transform);
        echo.onEchoCollision.AddListener(OnHeldBallCollision);
        echo.onEchoDeflected.AddListener((echo) => DropBall());
        ExitState();
    }

    void OnHeldBallCollision(BaseEcho echo)
    {
        if (heldBall == null) return;
        DropBall(false);
        RemoveSignals();
        character.SetLookTarget(heldBall.transform);
        character.velocityManager.AddExternalSpeed((heldBall.transform.position - character.transform.position).normalized * tacklePushback, "TakebackTackle");
    }

    void ThrowBall()
    {
        if (heldBall == null) return;
        heldBall.transform.parent = null; 
        heldBall.FindNewTarget(character);
        heldBall.EnableProjectile();
        yoyoTracker = yoyoDuration;
        currentState = TakebackState.Throwing;
        heldBall.UpdateSpeed(previousEchoSpeed);
        staminaComponent.ConsumeForesight();
        RemoveSignals();
        character.SetLookTarget(heldBall.transform);
        if(throwParticle != null)
        {
            throwParticle.transform.position = ballHolder.transform.position;
            if (enemySpeaker != null)
            {
                throwParticle.transform.LookAt(enemySpeaker.transform);
            }
            else
            {
                Debug.Log("couldn't find enemy speaker YIKIDIE");
            }
                throwParticle.Play();
        }

    }


    void DropBall(bool removeInvuln = true)
    {
        if (heldBall == null) return;
        if (removeInvuln) character.healthComponent.RemoveStatusEffect("TakebackTackle");
        heldBall.transform.parent = null;
        heldBall.EnableProjectile();
        currentState = TakebackState.None;
        heldBall.SetNewTarget(character);
        RemoveSignals();
        character.SetLookTarget(heldBall.transform);
    }
    void YoYoBall()
    {
        Debug.Log("Yoyoing");
        character.SetLookTarget(heldBall.transform);
        heldBall.SetNewTarget(character);
        yoyoTracker = 0.0f;
        StartCatch();
    }

    void RemoveSignals()
    {
        if (heldBall == null) return;
        heldBall.onEchoCollision.RemoveListener(OnHeldBallCollision);
        heldBall.onEchoDeflected.RemoveListener((echo) => DropBall());
    }

    void ExitState()
    {
        character.healthComponent.RemoveStatusEffect("TakebackTackle");
        if (!IsGrounded())
        {
            fsm.TransitionTo<FallState>();
        }
        else
        {

            if (GetMovementDir().magnitude < MOVE_DEADZONE)
            {
                fsm.TransitionTo<IdleState>();
            }
            else
            {
                fsm.TransitionTo<RunState>();
            }
        }
    }

    public override void ResetSkill()
    {
        currentState = TakebackState.None;
        yoyoTracker = 0.0f;
    }

   
}
