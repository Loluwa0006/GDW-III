using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{

    public Transform hitboxOwner;
    [SerializeField] int maxHealth = 3;
    int health = 3;

    public UnityEvent<DamageInfo> entityDamaged = new();
    public UnityEvent<DamageInfo, HealthComponent> entityDefeated = new();
    public UnityEvent<int> entityHealed = new();

    private void Awake()
    {
        health = maxHealth;
    }

    public virtual void Damage(DamageInfo info)
    {
        if (info.damage <= 0) { return; }
        health -= info.damage;

        Debug.Log("Dealing " + info.damage + " damage to entity " + hitboxOwner.name);
        if (health <= 0)
        {
            OnEntityDeath(info, this);
        }
        else
        {
            entityDamaged.Invoke(info);

        }
    }

    public virtual void OnEntityDeath(DamageInfo info, HealthComponent hp)
    {
        entityDefeated.Invoke(info, this);
    }

    public int GetHealth()
    {
        return health;
    }
 

}
