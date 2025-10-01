using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static HealthComponent;
public class HealthComponent : MonoBehaviour
{

    public enum StatusType
    {
        Invulnerability,
        Vulnerability,
        Armor,
        Slow,
        Stun,
        ReversedControls,
    }
    public enum DamageResult
    {
        InvincibleToType,
        Success,
        Armored,
        Weakened,
        Other
    }


    public Transform hurtboxOwner;
    public UnityEvent<DamageInfo> entityDamaged = new();
    public UnityEvent<DamageInfo, HealthComponent> entityDefeated = new();



    Dictionary<string, StatusEffect> statusEffects = new();

    public void AddStatusEffect(StatusType type, int duration, string ID)
    {
        statusEffects.Add(ID, new StatusEffect(type, duration));
    }

  

    public virtual DamageResult Damage(DamageInfo info)
    {
        if (info.damage <= 0) { return DamageResult.Other; }
        int originalDamage = info.damage;

        foreach (var effect in statusEffects.Values)
        {
            info.damage = effect.ModifyDamage(info);
        }

        if (info.damage < 0) info.damage = 0; //if damage is negative, entity heals, which is wrong;


            entityDamaged.Invoke(info);

        if (originalDamage > info.damage)
        {
            return DamageResult.Weakened;
        }
        else if (originalDamage < info.damage)
        {
            if (info.damage == 0)
            {
                return DamageResult.InvincibleToType;
            }
            return DamageResult.Armored;
        }
            return DamageResult.Success;
    }
    public virtual void OnEntityDeath(DamageInfo info, HealthComponent hp)
    {
        entityDefeated.Invoke(info, this);
    }


    private void FixedUpdate()
    {
        List<string> expiredEffects = new();
        foreach (var effect in statusEffects)
        {
            effect.Value.duration -= 1;
            if (effect.Value.duration <= 0)
            {
                expiredEffects.Add(effect.Key);
            }
        }

        foreach (var id in expiredEffects)
        {
            statusEffects[id].OnExpire();
            statusEffects.Remove(id);
        }
    }

}
public class StatusEffect
{
    public int duration = 0;
    public StatusType statusType;
    public bool removable = false;

    public StatusEffect(StatusType statusType, int duration, int amount = 0, bool removable = false)
    {
        this.duration = duration;
        this.statusType = statusType;
        this.removable = removable;
    }

    public virtual int ModifyDamage(DamageInfo info) => info.damage;
    public virtual void OnExpire()
    {

    }
}

public class InvulnerabilityEffect : StatusEffect
{
    public DamageType invincibilityType;
    protected InvulnerabilityEffect(DamageType type, int duration, StatusType statusType, int amount = 0, bool removable = false) : base(statusType, duration, amount, removable)
    {
        this.duration = duration;
        this.statusType = statusType;
        this.removable = removable;
    }
    public override int ModifyDamage(DamageInfo info)
    {
        if (info.damageType == invincibilityType)
        {
            return 0;
        }
        return info.damage;
    }
}

