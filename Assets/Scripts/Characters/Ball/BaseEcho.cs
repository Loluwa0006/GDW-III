using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]

public class BaseEcho : MonoBehaviour
{

    public HashSet<BaseSpeaker> characterList = new();
    public UnityEvent<BaseEcho> onEchoCollision = new();

    [HideInInspector] public bool ballActive = false;
    [HideInInspector] public bool isIgnited = false;

    [SerializeField] protected HitboxComponent hitbox;
    [SerializeField] protected Rigidbody _rb;
    [SerializeField] protected MeshRenderer mesh;
    [SerializeField] protected PlayerInput pInput;

    [Header("Speed Settings")]
    [SerializeField] protected float minSpeed;
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected float startingSpeed;
    [SerializeField] protected float igniteSpeed;
    [Header("Steer Settings")]
    [SerializeField] protected float minSteerForce;
    [SerializeField] protected float maxSteerForce;

   
    [SerializeField] protected int deflectsUntilMaxSpeed = 25;
    [SerializeField] protected float hitboxCooldown = 0.1f;

    [Header("Colors")]
    [SerializeField] Material normalColor;
    [SerializeField] Material igniteColor;

    [Header("Particles")]
    [SerializeField] protected ParticleSystem hitsparksLighting;
    [SerializeField] protected TrailRenderer echoTrail;
    [SerializeField] protected Gradient regularGradient;
    [SerializeField] protected Gradient ignitionGradient;
    [SerializeField] protected ParticleSystem ignitionTravelParticles;
    [SerializeField] protected ParticleSystem ignitionDeflectParticles;


    [Header("Contactstop")]
    [SerializeField] protected int hitstopAmount = 15;
    [SerializeField] protected int deflectstopAmount = 15;

     protected int deflectStreak = 0;

    protected float activeMinSpeed = 0;
    protected float activeMaxSpeed = 0;   
    protected float currentSpeed;
    protected float cooldownTracker = 0.0f;
    protected BaseSpeaker currentTarget;

    protected Vector2 startingPos;

    bool hitboxActive = true;



    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        if (hitbox == null)
        {
            hitbox = GetComponent<HitboxComponent>();
        }
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
        }
        if (pInput == null)
        {
            pInput = GetComponent<PlayerInput>();
        }
        hitbox.hitboxCollided.AddListener(OnHitboxCollided);
        startingPos = transform.position;
        activeMinSpeed = minSpeed;
        activeMaxSpeed = maxSpeed;
        SuspendProjectile();

    }

    private void Start()
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null )
        {
            if (!gameManager.echoList.Contains(this))
            {
                gameManager.echoList.Add(this);
            }
        }
        else
        {
            Debug.Log("Couldn't find game manager");
        }
    }

    protected virtual void OnHitboxCollided(HealthComponent hp)
    {
        if (!hitboxActive) { return; }
            if (hp.hurtboxOwner.TryGetComponent(out BaseSpeaker victim))
        {
            if (currentTarget != victim) { return; }
            hitbox.damageInfo.knockbackDir = (currentTarget.transform.position - transform.position).normalized;
            SetHitboxOnCooldown();
            onEchoCollision.Invoke(this);
            bool partial = victim.deflectManager.IsPartialDeflect();
            if (!victim.deflectManager.IsDeflecting())
            {
                OnPlayerHit(victim);
            }
            else if (partial && isIgnited)
            {
                OnPlayerHit(victim);
                victim.deflectManager.OnDeflectBroken();
            }
            else
            {
               OnDeflect(victim);
               StartCoroutine(victim.deflectManager.OnSuccessfulDeflect(this, partial));
            }
         
        }
    }

    void SetHitboxOnCooldown()
    {
        cooldownTracker = hitboxCooldown;
        hitboxActive = false;
    }
   public virtual void InitProjectile(HashSet<BaseSpeaker> charList)
    {
        if (charList.Count < 2) { return; }
        characterList = charList;
        currentTarget = characterList.ElementAt(0);
        transform.position = startingPos;

        EnableProjectile();
        UpdateSpeed(startingSpeed);
        deflectStreak = 0;
        activeMinSpeed = minSpeed;
        activeMaxSpeed = maxSpeed;
    }


    public void UpdateActiveCharacters(HashSet<BaseSpeaker> charList)
    {
        if (charList.Count < 2)
        {
            SuspendProjectile();
        }
        else if (!ballActive)
        {
            InitProjectile(charList);
        }
    }

    public void EnableProjectile()
    {
        mesh.enabled = true;
        ballActive = true;
        _rb.isKinematic = false;
    }


    public void SuspendProjectile(bool hide = true)
    {
        UpdateSpeed(0);
        mesh.enabled = !hide;
        ballActive = false;
        _rb.isKinematic = true;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.inSpecialStop) { return; }
        if (!ballActive || currentTarget == null) { return; }
        _rb.linearVelocity = (currentTarget.transform.position - transform.position).normalized * currentSpeed;
        transform.LookAt(currentTarget.transform.position);
    }


    private void Update()
    {
        if (cooldownTracker > 0.0f)
        {
            cooldownTracker -= Time.deltaTime;
            if (cooldownTracker <= 0.0f)
            {
                cooldownTracker = 0.0f;
                hitboxActive = true;
            }
        }
    }
    public void OnDeflect(BaseSpeaker characterWhoDeflectedBall)
    {
        Debug.Log("deflected");
        StartCoroutine(PostContactLogic(characterWhoDeflectedBall, false));
    }

    public void OnPlayerHit(BaseSpeaker character)
    {
      Debug.Log("hit");
      character.healthComponent.Damage(hitbox.damageInfo);
      OnPlayerCollision(character);
      StartCoroutine(PostContactLogic(character, true));
    }

    protected void PlayHitsparks()
    {
        //ParticleSystem lightingParticles = Instantiate(hitsparksLighting, null);
        //lightingParticles.transform.position = transform.position;
        //lightingParticles.Play();
    }

    protected void RemoveSpeedDuringHitstop()
    {
        if (_rb.isKinematic) { return; }
        Vector3 prevSpeed = _rb.linearVelocity;
        _rb.linearVelocity = Vector3.zero;
    }

    protected virtual IEnumerator PostContactLogic(BaseSpeaker cha, bool landedHit)
    {
        RemoveSpeedDuringHitstop();
        yield return null;
        if (landedHit)
        {
            PlayHitsparks();
            GameManager.ApplyHitstop(hitstopAmount);
        }
        else
        {
            float t = deflectStreak / (float)deflectsUntilMaxSpeed;
            deflectStreak += 1;
            UpdateSpeed(Mathf.Lerp(minSpeed, maxSpeed, t));
            GameManager.ApplyHitstop(deflectstopAmount);
            if (isIgnited)
            {
                ignitionDeflectParticles.Play();
            }
        }
        FindNewTarget(cha);
         

    }
    public void OnPartialDeflectIgnored(BaseSpeaker character)
    {
        character.healthComponent.Damage(hitbox.damageInfo);

        OnPlayerCollision(character);

        character.deflectManager.OnDeflectBroken();
    }

    public void OnPlayerCollision(BaseSpeaker character)
    {
        UpdateSpeed(minSpeed);
        if (character != null)
        {
            FindNewTarget(character);
            deflectStreak = 0;
        }
    }


    public virtual void FindNewTarget(BaseSpeaker lastHitCharacter)
    {
        HashSet<BaseSpeaker> targetList = new (characterList);
        targetList.Remove(lastHitCharacter);
        int randomIndex = Random.Range(0, targetList.Count);
        currentTarget = targetList.ElementAt(randomIndex);
    }

    public void SetNewTarget(BaseSpeaker target)
    {
        currentTarget = target;
    }

    public BaseSpeaker GetTarget()
    {
        return currentTarget;
    }

    public float GetSpeed()
    {
        return currentSpeed;
    }

   public virtual void UpdateSpeed(float newSpeed)
    {
        currentSpeed = Mathf.Clamp(newSpeed, activeMinSpeed, activeMaxSpeed);
        isIgnited = (currentSpeed >= igniteSpeed);
        if (igniteColor != null && normalColor != null)
        {
            mesh.material = isIgnited ? igniteColor : normalColor;
        }

        echoTrail.colorGradient = isIgnited ? ignitionGradient : regularGradient;
        if (!isIgnited)
        {
            ignitionTravelParticles.Stop();
        }
        else if (!ignitionTravelParticles.isPlaying) 
        {
            ignitionTravelParticles.Play();
        }
    }
    public void EnterSuddenDeath()
    {
        activeMinSpeed = igniteSpeed;
        if (currentSpeed < igniteSpeed)
        {
            UpdateSpeed(igniteSpeed);
        }
    }



    
}



