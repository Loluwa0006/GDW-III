using System.Collections.Generic;
using UnityEngine;

public class Takeback : BaseSkill
{
    enum TakebackState
    {
        Catching,
        Holding,
        Whiff,
    }

    TakebackState currentState = TakebackState.Catching;

    [SerializeField] float whiffDuration = 0.9f;

    float catchDuration;

    float durationTracker = 0.0f;

    float previousEchoSpeed = 0.0f;

    BaseEcho heldBall;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        catchDuration = cha.deflectManager.GetGoodDeflectDuration();
    }


    public override void Enter(Dictionary<string, object> msg = null)
    {
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
                    currentState = TakebackState.Whiff;
                }
                break;
        }
        
    }

    public override void OnCharacterHit(DamageInfo info)
    {
        if (currentState == TakebackState.Catching && info.damageSource == DamageSource.Ball) EnterHoldState(info);
        else base.OnCharacterHit(info);
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
    }

    void ThrowBall()
    {
       
    }
}
