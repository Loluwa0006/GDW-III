using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Counterslash : BaseSkill
{
    //counter slash is unique: it drains stamina as you charge it up. there's a flat cost when releasing the blade tho


    const int NUMBER_OF_DEFLECT_PARTICLE_OBJECTS = 5;


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
    [SerializeField] ParticleSystem specialDeflectParticles;
    [SerializeField] Color underchargedColor = Color.white;
    [SerializeField] Color chargedColor = Color.lightBlue;
    [Header("SFX")]
    [SerializeField] AudioSource sfxHandler;
    [SerializeField] AudioSource windSwirler;
    [SerializeField] AudioClip electricBurst;
    [SerializeField] AudioClip fullPower;
    [SerializeField] float burstVolume = 0.7f;
    [Header("Other")]
    [SerializeField] ProgressBar chargeMeter;


    BufferHelper deflectBuffer;



    float chargeTracker = 0;
    int frameTracker;

    GameManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    List<ParticleSystem> particlesList = new();

    private void Start()
    {
        var main = releaseParticles.main;
        main.startColor = chargedColor;
    }

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        manager = FindFirstObjectByType<GameManager>();

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
        windSwirler.Stop();
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        frameTracker = framesUntilStaminaDrain;
        chargeTracker = 0.0f;

        chargeMeter.SetDisplayStatus(true);
        deflectBuffer.Consume();
        chargeParticles.time = 0;
        chargeParticles.Play();
        windSwirler.Play();

    }
    public override void Process()
    {

        if (!skillAction.IsPressed())
        {

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

        bool chargedBefore = (chargeTracker == chargeDuration);
        chargeTracker += Time.deltaTime;
        if (chargeTracker > chargeDuration)
        {
            chargeTracker = chargeDuration;
        }
        bool nowCharged = (chargeTracker == chargeDuration);

        if (!chargedBefore && nowCharged)
        {
            sfxHandler.PlayOneShot(fullPower);
        }

        float chargeAsPercent = chargeTracker / chargeDuration;
        chargeMeter.SetProgress(chargeAsPercent);
        
        var emission = chargeParticles.emission;
        emission.rateOverTime = Mathf.Lerp(minWindTrails, maxWindTrails, chargeAsPercent);

        var main = chargeParticles.main;
        main.startColor = nowCharged ? chargedColor : underchargedColor;
    }

    void OnCounterslashReleased()
    {
        if (chargeTracker < chargeDuration) return; 
        else if (manager.echoList.Count <= 0) { Debug.Log("nothing to deflect mr/mrs " + character.name); return;  }
        int index = 0;
            foreach (var ball in manager.echoList)
            {
                if (ball.GetTarget() == character)
                {
                    ball.OnDeflect(character);
                    var particle = particlesList[index];
                    particle.transform.position = ball.transform.position;
                    particle.Play();
                }
            index++;
            }
        if (!staminaComponent.HasForesight()) staminaComponent.DamageStamina(staminaCost, 0, false);
        else staminaComponent.ConsumeForesight();
            StartCoroutine(ExitState());
        releaseParticles.Play();
        sfxHandler.PlayOneShot(electricBurst, burstVolume);
            
        
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
            bool foresight = staminaComponent.HasForesight();
            if (!foresight)
            {
                staminaComponent.DamageStamina(1, 0, false);
            
            if (staminaComponent.GetStamina() <= staminaCost)
            {
                StartCoroutine(ExitState());
            }
            }
        }
        Vector3 currentSpeed = character.velocityManager.GetInternalSpeed();
        character.velocityManager.OverwriteInternalSpeed(currentSpeed * decelValue);
    }

    public override void Exit()
    {
        windSwirler.Stop();
        chargeMeter.SetDisplayStatus(false);
        chargeParticles.Stop();
    }
}
