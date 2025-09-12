using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RicochetBall : MonoBehaviour
{
    public List<BaseCharacter> characterList;

    [HideInInspector] public bool ballActive = false;

    [SerializeField] BaseCharacter currentTarget;
    [SerializeField] HitboxComponent hitbox;
    [SerializeField] Rigidbody _rb;
    [SerializeField] MeshRenderer mesh;

    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float startingSpeed;
    [SerializeField] float minSteerForce;
    [SerializeField] float maxSteerForce;
    [SerializeField] int deflectsUntilMaxSpeed = 25;

    int deflectStreak = 0;

    float currentSpeed;

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
        hitbox.hitboxCollided.AddListener(OnHitboxCollided);
        startingPos = transform.position;
        SuspendBall();
    }

    void OnHitboxCollided(HealthComponent hp)
    {
        if (hp.hitboxOwner.TryGetComponent(out BaseCharacter victim))
        {
            OnPlayerHit(victim);
        }
    }
   public void InitBall(List<BaseCharacter> charList)
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
        if (!ballActive) { return; }
        _rb.linearVelocity = (currentTarget.transform.position - transform.position).normalized * currentSpeed;
        foreach (BaseCharacter character in characterList)
        {
            character.transform.LookAt(transform.position);
        }
    }

    public void OnDeflect(BaseCharacter characterWhoDeflectedBall, Vector2 moveDir)
    {
        Debug.Log("Deflected by char " + characterWhoDeflectedBall.name);
        float t = deflectStreak / (float) deflectsUntilMaxSpeed;
        currentSpeed += Mathf.Lerp(minSpeed, maxSpeed, t);
        deflectStreak += 1;
        FindNewTarget(characterWhoDeflectedBall);
    }

    public void OnPlayerHit(BaseCharacter character)
    {
        Debug.Log("character " + character.name + " got hit by the ball");
        currentSpeed = minSpeed;
        FindNewTarget(character);
        deflectStreak = 0; 
    }

    public void FindNewTarget(BaseCharacter lastHitCharacter)
    {
        List<BaseCharacter> targetList = new (characterList);
        targetList.Remove(lastHitCharacter);
        int randomIndex = Random.Range(0, targetList.Count);
        currentTarget = targetList.ElementAt(randomIndex);
    }

    
}



