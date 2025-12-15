using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class DamageInfo
{
    public const float USE_DEFAULT_HITSTUN_GRAVITY = -1.0f;
    public const float DEFAULT_HITSTUN_GRAVITY = 0.85f;

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
    public int hitstop = 25;
   [HideInInspector] public Transform attacker;
    public AudioClip hitSFX;

    public DamageInfo CloneInfo()
    {
        DamageInfo clone = new();
        clone.damage = damage;
        clone.maxStaminaDamage = maxStaminaDamage;
        clone.damageSource = damageSource;
        clone.dealsGrayStaminaDamage = dealsGrayStaminaDamage;

        clone.hitstun = hitstun;
        clone.knockbackDir = knockbackDir;
        clone.knockbackDistance = knockbackDistance;
        clone.hitstunGravity = hitstunGravity;
        clone.knockbackLaunch = knockbackLaunch;

        clone.leaveTargetInvincible = leaveTargetInvincible;
        clone.attacker = attacker;
        clone.hitSFX = hitSFX;
        clone.hitstop = hitstop;
        return clone;
    }

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

        if (damageInfo.attacker == null)
        {
           damageInfo.attacker = transform.parent;
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