using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class RicochetBall : MonoBehaviour
{

    [SerializeField] BaseCharacter currentTarget;
    [SerializeField] HitboxComponent hitbox;
    [SerializeField] Rigidbody _rb;


    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float startingSpeed;
    [SerializeField] float minSteerForce;
    [SerializeField] float maxSteerForce;
    [SerializeField] int deflectsUntilMaxSpeed = 25;

    
    int deflectStreak = 0;


    float currentSpeed;

   [SerializeField] List<BaseCharacter> characterList;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        if (hitbox != null)
        {
            hitbox = GetComponent<HitboxComponent>();
        }
        hitbox.hitboxCollided.AddListener(OnHitboxCollided);

    }

    void OnHitboxCollided(HealthComponent hp)
    {
        if (hp.hitboxOwner.TryGetComponent(out BaseCharacter victim))
        {
            OnPlayerHit(victim);
        }
    }
    void Start()
    {
        currentSpeed = startingSpeed;
        if (characterList.Count == 0)
        {
            characterList = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None).ToList();
        }
        currentTarget = characterList.ElementAt(0);
    }

    // Update is called once per frame
    void Update()
    {
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



