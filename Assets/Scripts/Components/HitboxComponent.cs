using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class DamageInfo
{
    public enum DamageType
    {
        Ball,
        Skill,
        Environment,
        Other
    }

    public Transform attacker;
    public int damage;
    public DamageType damageType;

}


public class HitboxComponent : MonoBehaviour
{
    BoxCollider hitbox;


    [SerializeField] DamageInfo damageInfo;
    public UnityEvent<HealthComponent> hitboxCollided = new();
    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out HealthComponent hp))
        {
            hp.Damage(damageInfo);
            hitboxCollided.Invoke(hp);
        }
    }
}
