using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public class RicochetBall : MonoBehaviour
{

    [SerializeField] BaseCharacter currentTarget;
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float startingSpeed;
    [SerializeField] float minSteerForce;
    [SerializeField] float maxSteerForce;
    [SerializeField] int deflectsUntilMaxSpeed = 25;
    int deflectStreak = 0;


    float currentSpeed;

    HashSet<BaseCharacter> characterList;
    HashSet<BaseCharacter> targetList = new();

    Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        currentSpeed = startingSpeed;
        characterList = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None).ToHashSet(); 
        currentTarget = characterList.ElementAt(0);
        targetList = new HashSet<BaseCharacter>(characterList);
        targetList.Remove(currentTarget);
    }

    // Update is called once per frame
    void Update()
    {
        _rb.linearVelocity = (currentTarget.transform.position - transform.position).normalized * currentSpeed;
    }

    public void OnDeflect()
    {
        float t = deflectStreak / (float) deflectsUntilMaxSpeed;
        currentSpeed += Mathf.Lerp(minSpeed, maxSpeed, t);
        deflectStreak += 1;
    }

    public void OnPlayerHit(BaseCharacter character)
    {
        currentSpeed = minSpeed;
        FindNewTarget(character);
        deflectStreak = 0;
    }

    public void FindNewTarget(BaseCharacter lastHitCharacter)
    {
        int randomIndex = Random.Range(0, targetList.Count - 1);
        currentTarget = targetList.ElementAt(randomIndex);
        targetList.Add(lastHitCharacter);
        targetList.Remove(currentTarget);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out BaseCharacter character))
        {
            OnPlayerHit(character);
        }
    }
}


