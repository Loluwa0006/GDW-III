using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR.Haptics;

public class Takeback : BaseSkill
{
    enum TakebackState
    {
        Catching,
        Whiff,
        Holding,
        Throwing,
    }

    TakebackState currentState = TakebackState.Catching;

    UnityAction<BaseEcho> onEchoCollision;
    UnityAction<BaseEcho> onEchoDeflectedDrop;
    UnityAction<BaseEcho> onEchoDeflectedYoyo;
    UnityAction<Vector3> onEchoWarped;

    [SerializeField] float whiffDuration = 0.9f;
    [SerializeField] float yoyoDuration = 2.7f; //amount of time after throwing you have to yo-yo
    [SerializeField] int holdStaminaDrainRate = 8;
    [SerializeField] float decelRate = 0.9f;
    [SerializeField] float tacklePushback = 8.0f;
    [SerializeField] int staminaFreeHoldFrames = 12;
    [SerializeField] int yoyoCost = 8;


    [SerializeField] Transform ballHolder;
    [Header("Particles")]
    [SerializeField] ParticleSystem catchParticle;
    [SerializeField] ParticleSystem throwParticle;
    [SerializeField] ParticleSystem catchAttemptParticle;
    [Header("SFX")]
    [SerializeField] AudioClip catchSFX;
    [SerializeField] AudioClip throwSFX;
    [Header("Other")]
    [SerializeField] LineRenderer yoyoLine;
    [SerializeField] Color catchAvailableColor;
    [SerializeField] Color whiffedCatchColor;
   

    float catchDuration; //determined by deflect duration


    float durationTracker = 0.0f;
    float whiffTracker = 0.0f;
    float yoyoTracker = 0.0f;
    int holdTracker;
    int freeHoldTracker = 0;

    float previousEchoSpeed = 0.0f;

    bool wasCatchingBeforeFreeze;
    bool freeCatch = false;

    BaseEcho heldBall;
    BaseSpeaker enemySpeaker;

    bool yoyoThisFrame = false;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        catchDuration = cha.deflectManager.GetGoodDeflectDuration();
        StartCoroutine(FindOppositeSpeaker());
        throwParticle.transform.SetParent(null);
        yoyoLine.gameObject.SetActive(false);;
        SetCatchAttemptParticleColorAndStopEmitting(catchAvailableColor);
    }

    IEnumerator FindOppositeSpeaker()
    {
        yield return new WaitForFixedUpdate();
        var speakers = FindObjectsByType<BaseSpeaker>(FindObjectsSortMode.None);
        foreach (var speaker in speakers)
        {
            if (speaker == character) { continue; }
            Debug.Log(character.name + " is looking at char " + speaker.name);
            enemySpeaker = speaker;
            break;
        }
    }


    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        EnterCatchState();
        if (!staminaComponent.HasForesight())
        {
            if (!freeCatch) staminaComponent.DamageStamina(staminaCost, 0, false);
            else freeCatch = false;
        }
        skillBuffer.Consume();
        ballHolder.parent = character.playerModel.transform;
        ballHolder.transform.position = character.deflectManager.transform.position;
    }


    public override void Process()
    {

        switch (currentState)
        {
            case TakebackState.Catching:
                durationTracker -= Time.deltaTime;
                if (durationTracker <= 0.0f)
                {
                    EnterWhiffState();
                }
                break;
            case TakebackState.Whiff:
                whiffTracker -= Time.deltaTime;
                if (whiffTracker <= 0.0f) ExitState();
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

    public override void PhysicsProcess()
    {
        var speed = character.velocityManager.GetInternalSpeed();
        speed *= decelRate;
        character.velocityManager.OverwriteInternalSpeed(speed);
    }


    public override void InactivePhysicsProcess() 
    {
        if (currentState == TakebackState.Holding)
        {
            holdTracker += 1;
            freeHoldTracker -= 1;
            if (freeHoldTracker < 0) freeHoldTracker = 0;
            Debug.Log("Free hold tracker == " + freeHoldTracker);
            Debug.Log("Hold tracker == " + holdTracker);
            if (holdTracker % holdStaminaDrainRate == 0 && freeHoldTracker <= 0)
            {
                if (!staminaComponent.HasForesight()) staminaComponent.DamageStamina(1, 0, false);
                if (staminaComponent.GetStamina() < staminaCost) DropBall();
            }
        }
    }

    public override void InactiveProcess()
    {
        if (!skillAction.IsPressed() && currentState == TakebackState.Holding)
        {
            EnterThrowState();
        }
        StartCoroutine(ClearYoyoBlocker());
        Debug.Log("Current state == " + currentState.ToString());
        YoyoLogic();
    }

    void YoyoLogic()
    {
        if (yoyoTracker <= 0 || currentState != TakebackState.Throwing)
        {
            yoyoLine.gameObject.SetActive(false);
        }
        else if (currentState == TakebackState.Throwing && staminaComponent.GetStamina() > yoyoCost && yoyoTracker > 0)
        {
            if (yoyoLine.gameObject.activeSelf) yoyoLine.SetPosition(1, heldBall.transform.position - character.transform.position);
            if (skillAction.WasPerformedThisFrame()) YoYoBall();
        }
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

    void EnterCatchState()
    {
        character.healthComponent.AddStatusEffect(new InvulnerabilityEffect(DamageSource.Ball, int.MaxValue, true), "TakebackCatch");   //it is infinite because we need full control over when it leaves
        durationTracker = catchDuration;
        currentState = TakebackState.Catching;
        SetCatchAttemptParticleColorAndStopEmitting(catchAvailableColor);
        catchAttemptParticle.Play();
    }

    void EnterHoldState(DamageInfo info)
    {
        if (!info.attacker.TryGetComponent(out BaseEcho echo))
        {
            Debug.LogWarning(echo.transform.name + " doesn't have ball component ");
            return;
        }
        if (yoyoThisFrame) return;
        freeHoldTracker = staminaFreeHoldFrames;
        holdTracker = 0;
        character.unscaledAudioSource.PlayOneShot(catchSFX);
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
        yoyoLine.gameObject.SetActive(false);;
        ConnectSignals(echo);
        ExitState();
        RemoveCatchAttemptParticles();
    }
    void EnterThrowState()
    {
        if (heldBall == null) return;
        character.unscaledAudioSource.PlayOneShot(throwSFX);
        EnableHeldEcho();
        heldBall.FindNewTarget(character);
        
        yoyoTracker = yoyoDuration;
        currentState = TakebackState.Throwing;
        heldBall.UpdateSpeed(previousEchoSpeed);
        staminaComponent.ConsumeForesight();
        RemoveSignals(false);
        if (throwParticle != null)
        {
            throwParticle.transform.position = ballHolder.transform.position;
            if (enemySpeaker != null)
            {
                throwParticle.transform.rotation = Quaternion.LookRotation(heldBall._rb.linearVelocity.normalized);
            }
            throwParticle.Play();
        }
        Debug.Log("entered throw state");
        yoyoLine.gameObject.SetActive(true);
    }

    void EnterWhiffState()
    {
        whiffTracker = whiffDuration;
        currentState = TakebackState.Whiff;
        SetCatchAttemptParticleColorAndStopEmitting(whiffedCatchColor);
        catchAttemptParticle.Play();
        character.healthComponent.RemoveStatusEffect("TakebackCatch");
    }

    void OnHeldBallCollision(BaseEcho echo)
    {
        if (heldBall == null) return;
        DropBall();
        RemoveSignals();
        character.healthComponent.AddStatusEffect(new InvulnerabilityEffect(DamageSource.Ball, 10, false), "TakebackPostSuccessfulTackle");// remove infinite, replace with temp

    }


    public void EnableHeldEcho()
    {
        heldBall.transform.parent = null;
        heldBall.EnableProjectile();
        character.SetLookTarget(heldBall.transform);
        character.healthComponent.RemoveStatusEffect("TakebackCatch");
    }

    void DropBall()
    {
        if (heldBall == null) return;
        EnableHeldEcho();
        currentState = TakebackState.Catching;
        heldBall.SetNewTarget(character);
        RemoveSignals();
    }
    void YoYoBall()
    {
        Debug.Log("Yoyoing");
        heldBall.SetNewTarget(character);
        yoyoTracker = 0.0f;
        freeCatch = true;
        yoyoThisFrame = true;
        yoyoLine.gameObject.SetActive(false);
        currentState = TakebackState.Catching;
        staminaComponent.DamageStamina(yoyoCost, 0, false);
    }

    IEnumerator ClearYoyoBlocker()
    {
        yield return null;
        yoyoThisFrame = false;
    }

    void OnHeldBallWarped(Vector3 movement)
    {
        StartCoroutine(PostWarpLogic(movement));
    }

    IEnumerator PostWarpLogic(Vector3 movement)
    {
        yield return null;
        character.transform.position += movement;
        if (heldBall != null)
        {
            heldBall.transform.localPosition = Vector3.zero;
        }
    }

    void OnThrownBallDeflected()
    {
        yoyoTracker = 0.0f; // no more yoyo if opponent deflects the ball
        yoyoLine.gameObject.SetActive(false);;
        freeCatch = false;
        Debug.Log("Thrown ball deflected, no more yoyo");
    }

    void ConnectSignals(BaseEcho echo)
    {
        onEchoCollision = OnHeldBallCollision;
        onEchoDeflectedDrop = _ => DropBall();
        onEchoDeflectedYoyo = _ => OnThrownBallDeflected();
        onEchoWarped = OnHeldBallWarped;

        heldBall.echoCollision.AddListener(onEchoCollision);
        heldBall.echoDeflected.AddListener(onEchoDeflectedDrop);
        heldBall.echoDeflected.AddListener(onEchoDeflectedYoyo);
        heldBall.echoWarped.AddListener(onEchoWarped);

    }
    void RemoveSignals(bool removeYoyo = true)
    {
        if (heldBall == null) return;
        heldBall.echoCollision.RemoveListener(onEchoCollision);
        heldBall.echoDeflected.RemoveListener(onEchoDeflectedDrop);
        if (removeYoyo) heldBall.echoDeflected.RemoveListener(onEchoDeflectedYoyo);
        heldBall.echoWarped.RemoveListener(onEchoWarped);
    }


    void ExitState()
    {
        if (!IsGrounded()) fsm.TransitionTo<FallState>();
        
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
    public override void Exit()
    {
        character.healthComponent.RemoveStatusEffect("TakebackCatch"); 
        SetCatchAttemptParticleColorAndStopEmitting(catchAvailableColor);
    }

    public void RemoveCatchAttemptParticles()
    {
        catchAttemptParticle.Clear();
        catchAttemptParticle.Stop();
    }

    public void SetCatchAttemptParticleColorAndStopEmitting(Color color)
    {
        RemoveCatchAttemptParticles();
        var main = catchAttemptParticle.main;
        main.startColor = color;
    }

    public override void ResetSkill()
    {
        currentState = TakebackState.Catching;
        yoyoTracker = 0.0f;
        yoyoLine.gameObject.SetActive(false);;
        SetCatchAttemptParticleColorAndStopEmitting(catchAvailableColor);
    }

    public override void OnSpecialStopStarted()
    {
        wasCatchingBeforeFreeze = fsm.currentState == this && currentState == TakebackState.Catching;
    }

    public override bool SkillAvailable()
    {
        if (!wasCatchingBeforeFreeze && (GameManager.inSpecialStop || GameManager.frameAfterSpecialStop))
        {
            return false; //can't deflect during freeze
        }
        return base.SkillAvailable() && !yoyoThisFrame;
    }


}
