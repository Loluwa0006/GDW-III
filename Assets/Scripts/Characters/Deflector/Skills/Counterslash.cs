using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Counterslash : BaseSkill
{
    //counter slash is unique: it drains stamina as you charge it up. there's a flat cost when releasing the blade tho


    [SerializeField] float chargeDuration = 1.5f;
    [SerializeField] float timeUntilCancel = 0.6f;
    [SerializeField] float stanceGravity = 0.01f;
    [SerializeField] float maxUpwardsVelocity = 5.0f;
    [SerializeField] int framesUntilStaminaDrain = 6;

    [SerializeField] Transform chargeMeterOver;
    [SerializeField] Transform chargeMeterUnder;
    [SerializeField] MeshRenderer chargeMeterMesh;
    [SerializeField] Material chargeMeterMax;
    [SerializeField] Material chargeMeterProgress;

    float originalChargeSize = 0.0f;


    float chargeTracker = 0;
    int frameTracker;

    GameManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    Rigidbody _rb;


    private void Start()
    {
        originalChargeSize = chargeMeterOver.transform.localScale.x;
    }

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        manager = FindFirstObjectByType<GameManager>();
        _rb = cha.GetComponent<Rigidbody>();

        chargeMeterOver.gameObject.SetActive(false);
        chargeMeterUnder.gameObject.SetActive(false);
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        frameTracker = framesUntilStaminaDrain;
        chargeTracker = 0.0f;
        _rb.linearVelocity = new (0, Mathf.Min(maxUpwardsVelocity, _rb.linearVelocity.y), 0);

        chargeMeterOver.gameObject.SetActive(true);
        chargeMeterUnder.gameObject.SetActive(true);


    }
    public override void Process()
    {
        if (skillAction.WasReleasedThisFrame())
        {
            Debug.Log("Attempting cancel, charge tracker is " + chargeTracker + ", time until cancel is " + timeUntilCancel);
            if (chargeTracker >= timeUntilCancel)
            {
               StartCoroutine(ExitState());
                return;
            }
        }
        if (playerInput.actions["Deflect"].WasPerformedThisFrame())
        {
            OnCounterslashReleased();
        }

        float chargeAsPercent = chargeTracker / chargeDuration;
        Vector3 newScale = chargeMeterOver.localScale;
        newScale.x = originalChargeSize * chargeAsPercent;
        chargeMeterOver.localScale = newScale;
        Vector3 newPos = chargeMeterOver.transform.localPosition;
        newPos.x = (originalChargeSize - newScale.x) / 2.0f;
        chargeMeterOver.transform.localPosition = newPos;

        chargeMeterMesh.material = chargeTracker >= chargeDuration ? chargeMeterMax : chargeMeterProgress;

        
    }

    void OnCounterslashReleased()
    {
        if (chargeTracker >= chargeDuration)
        {
            bool deflectedBall = false;
            foreach (var ball in manager.echoList)
            {
                if (ball.GetTarget() == character)
                {
                    deflectedBall = true;
                    ball.OnDeflect(character, GetMovementDir());
                }
            }
            if (!deflectedBall) return;
            staminaComponent.DamageStamina(staminaCost, false);
            StartCoroutine(ExitState());
        }
    }

    IEnumerator ExitState()
    {
        Debug.Log("exiting counterslash");
        yield return null;
        if (!IsGrounded())
        {
            fsm.TransitionTo<FallState>();
        }
        else
        {
            var moveDir = GetMovementDir();
            if (moveDir.magnitude >= MOVE_DEADZONE)
            {
                fsm.TransitionTo<RunState>();
            }
            else
            {
                fsm.TransitionTo<IdleState>();
            }
        }
    }

    

    // Update is called once per frame

    public override void PhysicsProcess()
    {
        chargeTracker += Time.fixedDeltaTime;
        if (chargeTracker > chargeDuration)
        {
            chargeTracker = chargeDuration;
        }
        Vector3 newSpeed = _rb.linearVelocity;
        newSpeed.y -= stanceGravity;
        _rb.linearVelocity = newSpeed;
        frameTracker--;
        if (frameTracker <= 0)
        {
            frameTracker = framesUntilStaminaDrain;
            staminaComponent.DamageStamina(1, false);
            if (staminaComponent.GetStamina() <= 1.01f)
            {
                StartCoroutine(ExitState());
            }
        }
    }

    public override void Exit()
    {
         chargeMeterOver.gameObject.SetActive(false);
        chargeMeterUnder.gameObject.SetActive(false);
    }
}

