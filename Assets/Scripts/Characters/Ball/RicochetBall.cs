using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public class RicochetBall : MonoBehaviour
{
    public HashSet<BaseCharacter> characterList = new();

    [HideInInspector] public bool ballActive = false;

    [SerializeField] HitboxComponent hitbox;
    [SerializeField] Rigidbody _rb;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] PlayerInput pInput;

    [Header("Speed Settings")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float startingSpeed;
    [Header("Steer Settings")]
    [SerializeField] float minSteerForce;
    [SerializeField] float maxSteerForce;

   
    [SerializeField] int deflectsUntilMaxSpeed = 25;
    [SerializeField] float hitboxCooldown = 0.1f;


    int deflectStreak = 0;

    float currentSpeed;
    BaseCharacter currentTarget;

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
        SuspendBall();
    }

    private void Start()
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.echoList.Add(this);
        }
        else
        {
            Debug.Log("Couldn't find game manager");
        }
    }

    void OnHitboxCollided(HealthComponent hp)
    {
        if (hp.hurtboxOwner.TryGetComponent(out BaseCharacter victim))
        {
            OnPlayerHit(victim);
            StartCoroutine(HitboxCooldown());
        }
    }

    IEnumerator HitboxCooldown()
    {
        hitbox.enabled = false;
        yield return new WaitForSeconds(hitboxCooldown);
        hitbox.enabled = true;
    }
   public void InitBall(HashSet<BaseCharacter> charList)
    {
        if (charList.Count < 2) { return; }
        characterList = charList;
        currentSpeed = startingSpeed;
        currentTarget = characterList.ElementAt(0);
        transform.position = startingPos;

        mesh.enabled = true;
        hitbox.enabled = true;
        ballActive = true;
        _rb.isKinematic = false;
    }

    public void UpdateActiveCharacters(HashSet<BaseCharacter> charList)
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
        currentSpeed = 0;
        mesh.enabled = false;
        hitbox.enabled = false;
        ballActive = false;
        _rb.isKinematic = true;
    }
    // Update is called once per frame
    void Update()
    {
        foreach (BaseCharacter character in characterList)
        {
            if (character != null) character.transform.LookAt(transform.position);
        }
        if (!ballActive || currentTarget == null) { return; }
        _rb.linearVelocity = (currentTarget.transform.position - transform.position).normalized * currentSpeed;
       
    }

    public void OnDeflect(BaseCharacter characterWhoDeflectedBall, Vector2 moveDir)
    {
        float t = deflectStreak / (float) deflectsUntilMaxSpeed;
        deflectStreak += 1;
        currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        FindNewTarget(characterWhoDeflectedBall);

        StartCoroutine(HitboxCooldown());

        Debug.Log("Deflected by char " + characterWhoDeflectedBall.name + ", streak is now " + deflectStreak + ", t is " + t);
    }

    public void OnPlayerHit(BaseCharacter character)
    {
        if (character != currentTarget) { return; }
        Debug.Log("character " + character.name + " got hit by the ball");
        currentSpeed = minSpeed;
        FindNewTarget(character);
        deflectStreak = 0; 
    }

    public void FindNewTarget(BaseCharacter lastHitCharacter)
    {
        HashSet<BaseCharacter> targetList = new (characterList);
        targetList.Remove(lastHitCharacter);
        int randomIndex = Random.Range(0, targetList.Count);
        currentTarget = targetList.ElementAt(randomIndex);
    }

    public BaseCharacter GetTarget()
    {
        return currentTarget;
    }



    
}



