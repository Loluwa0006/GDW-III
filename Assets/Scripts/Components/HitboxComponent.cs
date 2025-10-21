using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class DamageInfo
{

    public const float USE_DEFAULT_HITSTUN_GRAVITY = -1.0f;
    public const float DEFAULT_HITSTUN_GRAVITY = 0.85f;

    [HideInInspector] public Transform attacker;

    [Header("Damage")]
    public int damage;
    public int maxStaminaDamage;
    public DamageSource damageSource;
    public bool dealsGrayStaminaDamage = false;

    public int hitstun = 0;
    [Header("Knockback")]
    public Vector3 knockbackDir = Vector3.one.normalized;
    public float knockbackDistance = 5.0f;
    public float hitstunGravity = USE_DEFAULT_HITSTUN_GRAVITY;
    public float knockbackLaunch = 2.0f;

    [Header("Other")]
    public bool leaveTargetInvincible = true;
    


}


public class HitboxComponent : MonoBehaviour
{
     public Collider hitboxCollider;

    public DamageInfo damageInfo;
    
    public UnityEvent<HealthComponent> hitboxCollided = new();
    private void Awake()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out HealthComponent hp))
        { 
            Debug.Log(transform.parent + " collided with " + hp.transform.name);
            hitboxCollided.Invoke(hp);
        }
    }
}
[System.Serializable]
public enum DamageSource
{
    Ball,
    Skill,
    Environment,
    Other
}