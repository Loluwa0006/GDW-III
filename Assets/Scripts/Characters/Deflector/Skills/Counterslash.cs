using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Counterslash : BaseSkill
{
    //counter slash is unique: it drains stamina as you charge it up. there's a flat cost when releasing the blade tho


    const int NUMBER_OF_DEFLECT_PARTICLE_OBJECTS = 3;


    [Header("Balance Attributes")]
    [SerializeField] float chargeDuration = 1.5f;
    [SerializeField] float timeUntilCancel = 0.6f;
    [SerializeField] int framesUntilStaminaDrain = 6;
    [SerializeField] float decelValue = 0.95f;
    [Header("Particles")]
    [SerializeField] int minWindTrails = 10;
    [SerializeField] int maxWindTrails = 50;
    [SerializeField] ParticleSystem chargeParticles;
    [SerializeField] ParticleSystem releaseParticles;
    [SerializeField] Color underchargedColor = Color.white;
    [SerializeField] Color chargedColor = Color.lightBlue;
    [Header("")]
    [SerializeField] Transform chargeMeterOver;
    [SerializeField] Transform chargeMeterUnder;
    [SerializeField] MeshRenderer chargeMeterMesh;
    [SerializeField] Material chargeMeterMax;
    [SerializeField] Material chargeMeterProgress;
    [SerializeField] ParticleSystem specialDeflectParticles;


    BufferHelper deflectBuffer;

    float originalChargeSize = 0.0f;


    float chargeTracker = 0;
    int frameTracker;

    GameManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    List<ParticleSystem> particlesList = new();

    private void Start()
    {
        originalChargeSize = chargeMeterOver.transform.localScale.x;
        var main = releaseParticles.main;
        main.startColor = chargedColor;
    }

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        manager = FindFirstObjectByType<GameManager>();

        chargeMeterOver.gameObject.SetActive(false);
        chargeMeterUnder.gameObject.SetActive(false);
        deflectBuffer = s_machine.TryGetBuffer("DeflectBuffer");
        if (deflectBuffer == null)
        {
            Debug.LogError("Character " + cha + " missing deflect buffer");
        }

        for (int i = 0; i < NUMBER_OF_DEFLECT_PARTICLE_OBJECTS; i++) 
        {
            var particles = Instantiate(specialDeflectParticles, transform);
            particlesList.Add(particles);
            particles.Stop();
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
        chargeParticles.time = 0;
        chargeParticles.Play();


    }
    public override void Process()
    {

        if (!skillAction.IsPressed())
        {
            Debug.Log("Attempting cancel, charge tracker is " + chargeTracker + ", time until cancel is " + timeUntilCancel);

            if (chargeTracker >= chargeDuration)
            {
                OnCounterslashReleased();
            }
            else if (chargeTracker >= timeUntilCancel)
            {
                StartCoroutine(ExitState());
            }
        }
        else if (oppositeSkillBuffer != null) 
        {
            if (oppositeSkillBuffer.Buffered)
            {
                fsm.TransitionToSkill(oppositeSkillIndex);
                return;
            }
        }


        ChargeMeterLogic();
        
    }

    void ChargeMeterLogic()
    {

        chargeTracker += Time.deltaTime;
        if (chargeTracker > chargeDuration)
        {
            chargeTracker = chargeDuration;
        }

        float chargeAsPercent = chargeTracker / chargeDuration;
        Vector3 newScale = chargeMeterOver.localScale;
        newScale.x = originalChargeSize * chargeAsPercent;
        chargeMeterOver.localScale = newScale;
        Vector3 newPos = chargeMeterOver.transform.localPosition;
        newPos.x = (originalChargeSize - newScale.x) / 2.0f;
        chargeMeterOver.transform.localPosition = newPos;

        chargeMeterMesh.material = chargeTracker >= chargeDuration ? chargeMeterMax : chargeMeterProgress;

        var emission = chargeParticles.emission;
        emission.rateOverTime = Mathf.Lerp(minWindTrails, maxWindTrails, chargeAsPercent);

        var main = chargeParticles.main;
        main.startColor = chargeAsPercent > 0.99f ? chargedColor : underchargedColor;
    }

    void OnCounterslashReleased()
    {
        if (chargeTracker < chargeDuration) { Debug.Log("not enough charge for counterslash mr " + character.name); return;  }
        else if (manager.echoList.Count <= 0) { Debug.Log("nothing to deflect mr " + character.name); return;  }
        int index = 0;
            foreach (var ball in manager.echoList)
            {
                if (ball.GetTarget() == character)
                {
                    ball.OnDeflect(character);
                var particle = particlesList[index];
                particle.transform.position = ball.transform.position;
                particle.time = 0;
                particle.Play();
                }
            index++;
            }
            staminaComponent.DamageStamina(staminaCost, 0, false);
            StartCoroutine(ExitState());
        chargeParticles.Stop();
        releaseParticles.time = 0;
        releaseParticles.Play();
            
        
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
    
        frameTracker--;
        if (frameTracker <= 0)
        {
            frameTracker = framesUntilStaminaDrain;
            if (!staminaComponent.HasForesight()) staminaComponent.DamageStamina(1, 0, false);
            if (staminaComponent.GetStamina() <= staminaCost)
            {
                StartCoroutine(ExitState());
            }
        }
        Vector3 currentSpeed = character.velocityManager.GetInternalSpeed();
        character.velocityManager.OverwriteInternalSpeed(currentSpeed * decelValue);
    }

    public override void Exit()
    {
         chargeMeterOver.gameObject.SetActive(false);
        chargeMeterUnder.gameObject.SetActive(false);
        chargeParticles.Stop();
    }
}

