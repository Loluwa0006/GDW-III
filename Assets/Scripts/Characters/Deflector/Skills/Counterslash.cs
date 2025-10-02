using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Counterslash : BaseSkill
{
    //counter slash is unique: it drains stamina as you charge it up. there's a flat cost when releasing the blade tho


    [SerializeField] float chargeDuration = 1.5f;
    [SerializeField] float timeUntilCancel = 0.6f;
    [SerializeField] int framesUntilStaminaDrain = 6;
    [SerializeField] float decelValue = 0.95f;

    [SerializeField] Transform chargeMeterOver;
    [SerializeField] Transform chargeMeterUnder;
    [SerializeField] MeshRenderer chargeMeterMesh;
    [SerializeField] Material chargeMeterMax;
    [SerializeField] Material chargeMeterProgress;

    BufferHelper deflectBuffer;

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
        deflectBuffer = s_machine.TryGetBuffer("DeflectBuffer");
        if (deflectBuffer == null)
        {
            Debug.LogError("Character " + cha + " missing deflect buffer");
        }
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        frameTracker = framesUntilStaminaDrain;
        chargeTracker = 0.0f;

        chargeMeterOver.gameObject.SetActive(true);
        chargeMeterUnder.gameObject.SetActive(true);
        deflectBuffer.Consume();


    }
    public override void Process()
    {
     
        if (skillAction.WasReleasedThisFrame())
        {
            Debug.Log("Attempting cancel, charge tracker is " + chargeTracker + ", time until cancel is " + timeUntilCancel);

            if (chargeTracker >= chargeDuration)
            {
                OnCounterslashReleased();
            }
            else if (chargeTracker >= timeUntilCancel)
            {
             StartCoroutine (ExitState());
            }
            return;

        }
        else if (oppositeSkillBuffer.Buffered)
        {
            fsm.TransitionToSkill(oppositeSkillIndex);
            return;
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
        if (chargeTracker < chargeDuration) { Debug.Log("not enough charge for counterslash mr " + character.name); return;  }
        
            bool deflectedBall = false;
            foreach (var ball in manager.echoList)
            {
                if (ball.GetTarget() == character)
                {
                    deflectedBall = true;
                    ball.OnDeflect(character);
                }
            }
            if (!deflectedBall) return;
            staminaComponent.DamageStamina(staminaCost, false);
            StartCoroutine(ExitState());
        
        
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
        _rb.linearVelocity *= decelValue;

    }

    public override void Exit()
    {
         chargeMeterOver.gameObject.SetActive(false);
        chargeMeterUnder.gameObject.SetActive(false);
    }
}

