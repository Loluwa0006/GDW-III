using System.Collections.Generic;
using UnityEngine;

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


    [SerializeField] Transform ballHolder;
    float catchDuration;

    float durationTracker = 0.0f;
    float whiffTracker = 0.0f;
    float yoyoTracker = 0.0f;
    int holdTracker;

    float previousEchoSpeed = 0.0f;

    BaseEcho heldBall;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        catchDuration = cha.deflectManager.GetGoodDeflectDuration();
    }


    public override void Enter(Dictionary<string, object> msg = null)
    {
        StartCatch();
        staminaComponent.DamageStamina(staminaCost, 0, false);
        currentState = TakebackState.Catching;
        durationTracker = catchDuration;
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
        
    }

    public override void InactivePhysicsProcess()
    {

        switch (currentState)
        {
            case TakebackState.Holding:
                holdTracker += 1;
                if (holdTracker == holdStaminaDrainRate)
                {
                    staminaComponent.DamageStamina(1, 0, false);
                    if (staminaComponent.GetStamina() < staminaCost)
                    {
                        DropBall();
                    }
                }
                break;
            case TakebackState.Throwing:
                if (skillBuffer.Buffered && CanYoyo())
                {
                    YoYoBall();
                }
                yoyoDuration -= Time.deltaTime;
                break;
        }
      
    }

    public override void InactiveProcess()
    {
            if (!skillAction.IsPressed() && currentState == TakebackState.Holding && heldBall != null)
            {
                ThrowBall();
            }
        }

    public override void PhysicsProcess()
    {
        var speed = character.velocityManager.GetInternalSpeed();
        speed *= decelRate;
        character.velocityManager.OverwriteInternalSpeed(speed);
    }


    public override void OnCharacterHit(DamageInfo info)
    {
        if (currentState == TakebackState.Catching && info.damageSource == DamageSource.Ball) EnterHoldState(info);
        else base.OnCharacterHit(info);
    }

    void StartCatch()
    {
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
        echo.SuspendProjectile(false);
        currentState = TakebackState.Holding;
        echo.transform.parent = ballHolder.transform;
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
        
    }

    void DropBall()
    {
        if (heldBall == null) return;
        heldBall.transform.parent = null;
        heldBall.EnableProjectile();
        currentState = TakebackState.None;
    }
    void YoYoBall()
    {
        heldBall.SetNewTarget(character);
        StartCatch();
    }

    bool CanYoyo()
    {
        return staminaComponent.GetStamina() > yoyoCost && yoyoTracker > 0.0f;
    }

    void ExitState()
    {
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
}
