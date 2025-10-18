using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static HealthComponent;
public class HealthComponent : MonoBehaviour
{
    [System.Serializable]
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

    bool playerDead = false;


    private void Start()
    {
        entityDamaged.AddListener(OnEntityDamaged);
    }
    public void AddStatusEffect(StatusEffect effect, string ID)
    {
        if (statusEffects.ContainsKey(ID))
        {
            return;
        }
        else
        {
            statusEffects.Add(ID, effect);
        }
     }
    public virtual DamageResult Damage(DamageInfo info)
    {
        if (info.damage <= 0) { return DamageResult.Other; }
        int originalDamage = info.damage;
        int currentDamage = info.damage;
        Debug.Log("OG damage = " + originalDamage);

        foreach (var effect in statusEffects.Values)
        {
            currentDamage = effect.ModifyDamage(info);
        }
        Debug.Log("new damage = " + info.damage);

        if (currentDamage <= 0) currentDamage = 0; //if damage is negative, entity heals, which is wrong;
        else entityDamaged.Invoke(info);

        if (originalDamage > currentDamage)
        {
            Debug.Log("Weakened, taking extra dmg");
            return DamageResult.Weakened;
        }
        else if (originalDamage < currentDamage)
        {
            if (currentDamage == 0)
            {
                Debug.Log("invuln to type " + info.damageSource);
                return DamageResult.InvincibleToType;
            }
            Debug.Log("armored, taking less dmg");
            return DamageResult.Armored;
        }
        Debug.Log("taking normal damage");
            return DamageResult.Success;
    }
    public virtual void KillEntity(DamageInfo info, HealthComponent hp)
    {
        entityDefeated.Invoke(info, this);
        playerDead = true;
    }

    public void OnEntityDamaged(DamageInfo info)
    {
        if (playerDead) { Debug.Log("Player " + hurtboxOwner.name + " is dead.");  return; }
        if (hurtboxOwner.TryGetComponent(out BaseSpeaker speaker))
        {
            Dictionary<string, object> msg = new()
            {
                ["Data"] = info
            };
            speaker.characterStateMachine.TransitionTo<GetHitState>(msg);
        }
    }

    private void FixedUpdate()
    {
        List<string> expiredEffects = new();

       if (!GameManager.inSpecialStop)
        {
            foreach (var effect in statusEffects)
            {

                effect.Value.duration -= 1;
                Debug.Log("decreased status effect " + effect.Key + " to new duration " +  effect.Value.duration);
                if (effect.Value.duration <= 0)
                {
                    Debug.Log(effect.Key + " has expired");

                    expiredEffects.Add(effect.Key);
                }
            }
        }

        foreach (var id in expiredEffects)
        {
            statusEffects[id].OnExpire();
            statusEffects.Remove(id);
            Debug.Log("Removed status effect " + id);
        }
    }

    public bool IsAlive()
    {
        return !playerDead;
    }

    public void ResetComponent()
    {
        playerDead = false;
        statusEffects.Clear();
    }
}

[System.Serializable]
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
[System.Serializable]
public class InvulnerabilityEffect : StatusEffect
{
    public DamageSource invincibilityType;

    public InvulnerabilityEffect(DamageSource type, int duration, bool removable = false)
        : base(StatusType.Invulnerability, duration, 0, removable)
    {
        invincibilityType = type;
    }

    public override int ModifyDamage(DamageInfo info)
    {
        if (info.damageSource == invincibilityType)
            return 0;
        return info.damage;
    }
}


