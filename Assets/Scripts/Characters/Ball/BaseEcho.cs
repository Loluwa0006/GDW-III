using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;


public class BaseEcho : MonoBehaviour
{

    public HashSet<BaseSpeaker> characterList = new();

    [HideInInspector] public bool ballActive = false;
    [HideInInspector] public bool isIgnited = false;

    [SerializeField] HitboxComponent hitbox;
    [SerializeField] Rigidbody _rb;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] PlayerInput pInput;

    [Header("Speed Settings")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float startingSpeed;
    [SerializeField] float igniteSpeed;
    [Header("Steer Settings")]
    [SerializeField] float minSteerForce;
    [SerializeField] float maxSteerForce;

   
    [SerializeField] int deflectsUntilMaxSpeed = 25;
    [SerializeField] float hitboxCooldown = 0.1f;

    [Header("Colors")]
    [SerializeField] Material normalColor;
    [SerializeField] Material igniteColor;

    [Header("Particles")]
    [SerializeField] ParticleSystem hitsparksLighting;
    [SerializeField] TrailRenderer echoTrail;
    [SerializeField] Gradient regularGradient;
    [SerializeField] Gradient ignitionGradient;


    [Header("Contactstop")]
    [SerializeField] int hitstopDuration = 15;
    [SerializeField] int parryDuration = 15;

    int deflectStreak = 0;

    float activeMinSpeed = 0;
    float activeMaxSpeed = 0;   
    float currentSpeed;
    float cooldownTracker = 0.0f;
    BaseSpeaker currentTarget;

    Vector2 startingPos;



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
        SuspendBall();

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

    void OnHitboxCollided(HealthComponent hp)
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
   public void InitBall(HashSet<BaseSpeaker> charList)
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
            SuspendBall();
        }
        else if (!ballActive)
        {
            InitBall(charList);
        }
    }

    public void SuspendBall()
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
        StartCoroutine(PostDeflectLogic(characterWhoDeflectedBall));
    }

    public void OnPlayerHit(BaseSpeaker character)
    {
      character.healthComponent.Damage(hitbox.damageInfo);
      OnPlayerCollision(character);
      StartCoroutine(PostHitEffects(character));
    }

    IEnumerator PostHitEffects(BaseSpeaker cha)
    {
        Vector3 prevSpeed = _rb.linearVelocity;
        _rb.linearVelocity = Vector3.zero;
        yield return null;
        ParticleSystem lightingParticles = Instantiate(hitsparksLighting, null);
        lightingParticles.transform.position = transform.position;
        lightingParticles.Play();
        GameManager.ApplyHitstop(hitstopDuration);
        FindNewTarget(cha);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        _rb.linearVelocity = prevSpeed;
    }

    IEnumerator PostDeflectLogic(BaseSpeaker cha)
    {

        Vector3 prevSpeed = _rb.linearVelocity;
        _rb.linearVelocity = Vector3.zero;
        GameManager.ApplyHitstop(parryDuration);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        float t = deflectStreak / (float)deflectsUntilMaxSpeed;
        deflectStreak += 1;
        UpdateSpeed(Mathf.Lerp(minSpeed, maxSpeed, t));
        FindNewTarget(cha);
        Debug.Log("Deflected by char " + cha.name + ", streak is now " + deflectStreak + ", t is " + t);

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
        FindNewTarget(character);
        deflectStreak = 0;
    }


    public void FindNewTarget(BaseSpeaker lastHitCharacter)
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


    void UpdateSpeed(float newSpeed)
    {
        currentSpeed = Mathf.Clamp(newSpeed, activeMinSpeed, activeMaxSpeed);
        isIgnited = (currentSpeed >= igniteSpeed);
        mesh.material = isIgnited ? igniteColor : normalColor;

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



