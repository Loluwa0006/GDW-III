using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
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

    public void RemoveStatusEffect(string ID)
    {
        if (statusEffects.ContainsKey(ID))
        {
            if (statusEffects[ID].removable)
            {
                statusEffects.Remove(ID);
            }
        }
    }
    public bool IsInvulnerableTo(DamageSource source)
    {
        foreach (var effect in statusEffects.Values)
        {
            if (effect.statusType == StatusType.Invulnerability)
            {
                var invuln = effect as InvulnerabilityEffect;
                if (invuln.invincibilityType == source)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public virtual DamageResult Damage(DamageInfo originalInfo)
    {
        if (originalInfo.damage <= 0) { return DamageResult.Other; }

        DamageInfo modifiedInfo = originalInfo.CloneInfo();
        Debug.Log("OG damage = " + originalInfo.damage);

        foreach (var effect in statusEffects.Values)
        {
            modifiedInfo.damage = effect.ModifyDamage(originalInfo);
        }
        Debug.Log("new damage = " + modifiedInfo.damage);
        if (modifiedInfo.damage <= 0) modifiedInfo.damage = 0; //if damage is negative, entity heals, which is wrong;
        else if (!playerDead) entityDamaged.Invoke(modifiedInfo);
            
        OnEntityDamaged(modifiedInfo);

        if (originalInfo.damage > modifiedInfo.damage)
        {
            Debug.Log("Weakened, taking extra dmg");
            return DamageResult.Weakened;
        }
        else if (originalInfo.damage < modifiedInfo.damage)
        {
            if (modifiedInfo.damage == 0)
            {
                Debug.Log("invuln to type " + originalInfo.damageSource);
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
        if (!hurtboxOwner.TryGetComponent(out BaseSpeaker speaker)) return;
        
        if (speaker.characterStateMachine.currentState.OnCharacterHit(info))
        {
            Vector3 currentSpeed = speaker.velocityManager.GetInternalSpeed();
            currentSpeed.y = info.knockbackLaunch;
            Vector3 knockbackVector = new Vector3(info.knockbackDir.x, currentSpeed.y, info.knockbackDir.z).normalized * info.knockbackDistance;
            speaker.velocityManager.OverwriteInternalSpeed(knockbackVector);
           
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
                //Debug.Log("decreased status effect " + effect.Key + " to new duration " +  effect.Value.duration);
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


