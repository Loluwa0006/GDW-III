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



    [System.Flags]
    public enum DamageResult
    {
        InvincibleToType,
        Success,
        Armored,
        Weakened,
        Other
    }
    DamageInfo.DamageType invincibilityType = 0;

    int armor = 0;

    private void Awake()
    {
        health = maxHealth;
    }

    public void AddInvincibilityType(DamageInfo.DamageType type)
    {
        invincibilityType |= type; 
    }

    public void AddDamageReduction(int amount)
    {
        armor += amount;
    }

    public void RemoveInvincibilityType(DamageInfo.DamageType type)
    {
        invincibilityType &= ~type;
    }
    public void RemoveDamageReduction(int amount)
    {
        armor -= amount;
        if (armor < 0) armor = 0;
    }

    public void AddVulnerability(int amount)
    {
        armor -= amount;
    }

    public void ResetArmor()
    {
        armor = 0;
    }

    public void ResetInvincibility()
    {
        invincibilityType = 0;
    }
    public virtual DamageResult Damage(DamageInfo info)
    {
        if (info.damage <= 0) { return DamageResult.Other; }
        else if ((invincibilityType & info.damageType) !=0 ) //if the flag is the same as the damage type, we're invincible, take no dmg
        { 
            return DamageResult.InvincibleToType; 
        }

        info.damage -= armor; //subtract armor from damage taken
        if (info.damage < 0) info.damage = 0; //if damage is negative, entity heals, which is wrong;

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
        if (armor == 0)
        {
            return DamageResult.Success;
        }
        else if (armor > 0)
        {
            return DamageResult.Armored;
        }
        else
        {
            return DamageResult.Weakened;
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
