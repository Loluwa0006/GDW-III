using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]

public class BaseEcho : MonoBehaviour
{

    public HashSet<BaseSpeaker> characterList = new();

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
        if (hp.hurtboxOwner.TryGetComponent(out BaseSpeaker victim))
        {
            if (currentTarget != victim) { return; }
            hitbox.damageInfo.knockbackDir = (currentTarget.transform.position - transform.position).normalized;
            SetHitboxOnCooldown();
            if (isIgnited && victim.deflectManager.IsPartialDeflect()) 
            {
                OnPlayerHit(victim);
                victim.deflectManager.OnDeflectBroken();
            }
            else if (victim.deflectManager.IsDeflecting() && !victim.deflectManager.IsPartialDeflect())
            {
                OnDeflect(victim);
                StartCoroutine(victim.deflectManager.OnSuccessfulDeflect(this));
            }
            else if (victim.deflectManager.IsDeflecting() && victim.deflectManager.IsPartialDeflect() && !isIgnited)
            {
                OnDeflect(victim);
                StartCoroutine( victim.deflectManager.OnSuccessfulDeflect(this, true));
            }
            else 
            {
                OnPlayerHit(victim);
            }
         
        }
    }

    void SetHitboxOnCooldown()
    {
        cooldownTracker = hitboxCooldown;
        hitbox.enabled = false;
    }
   public virtual void InitProjectile(HashSet<BaseSpeaker> charList)
    {
        if (charList.Count < 2) { return; }
        characterList = charList;
        currentTarget = characterList.ElementAt(0);
        transform.position = startingPos;

        mesh.enabled = true;
        hitbox.enabled = true;
        ballActive = true;
        _rb.isKinematic = false;
        activeMinSpeed = minSpeed;
        activeMaxSpeed = maxSpeed;
        deflectStreak = 0;
        UpdateSpeed(startingSpeed);

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

    public void SuspendProjectile()
    {
        UpdateSpeed(0);
        mesh.enabled = false;
        hitbox.enabled = false;
        ballActive = false;
        _rb.isKinematic = true;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.inSpecialStop) { return; }
        foreach (BaseSpeaker character in characterList)
        {
            if (character != null) character.playerModel.transform.LookAt(transform.position);
        }
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
                hitbox.enabled = true;
            }
        }
    }
    public void OnDeflect(BaseSpeaker characterWhoDeflectedBall)
    {
        StartCoroutine(PostContactLogic(characterWhoDeflectedBall, false));
    }

    public void OnPlayerHit(BaseSpeaker character)
    {
      character.healthComponent.Damage(hitbox.damageInfo);
      OnPlayerCollision(character);
      StartCoroutine(PostContactLogic(character, true));
    }

    protected void PlayHitsparks()
    {
        ParticleSystem lightingParticles = Instantiate(hitsparksLighting, null);
        lightingParticles.transform.position = transform.position;
        lightingParticles.Play();
    }

    protected void RemoveSpeedDuringHitstop()
    {
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

    public BaseSpeaker GetTarget()
    {
        return currentTarget;
    }


   protected virtual void UpdateSpeed(float newSpeed)
    {
        currentSpeed = Mathf.Clamp(newSpeed, activeMinSpeed, activeMaxSpeed);
        isIgnited = (currentSpeed >= igniteSpeed);
        if (igniteColor != null && normalColor != null)
        {
            mesh.material = isIgnited ? igniteColor : normalColor;
        }

        echoTrail.colorGradient = isIgnited ? ignitionGradient : regularGradient;
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



