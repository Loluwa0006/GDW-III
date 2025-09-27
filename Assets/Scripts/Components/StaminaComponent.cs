using System.Collections;
using UnityEngine;

public class StaminaComponent : MonoBehaviour
{


    [SerializeField] HealthComponent healthComponent;
    [SerializeField] DeflectManager deflectManager;

    const float STAMINA_REGEN_RATE = 7.5f;
    const float STAMINA_USAGE_REGEN_DELAY = 1.5f;
    const int BALL_MAX_STAMINA_DAMAGE = 25;
    const int DANGER_ZONE_PERCENT = 25;
    const int PARTIAL_DEFLECT_STAMINA_DAMAGE = 20;

    float stamina = 100f;
    float maxStamina = 100f;
    float grayStamina = 0.0f;

    bool regenStamina = true;
    bool inDangerZone = false;

    private void Start()
    {
        if (healthComponent == null)
        {
            healthComponent = transform.parent.GetComponentInChildren<HealthComponent>();
        }
        if (deflectManager == null)
        {
            deflectManager = transform.parent.GetComponentInChildren<DeflectManager>();
        }
        healthComponent.entityDamaged.AddListener(OnPlayerStruck);
        deflectManager.deflectedBall.AddListener(OnBallDeflected);
    }

    private void Update()
    {
        if (regenStamina)
        {
            float staToAdd = STAMINA_REGEN_RATE * Time.deltaTime;
            stamina += staToAdd;
            if (stamina > maxStamina) { stamina = maxStamina; }
            grayStamina -= staToAdd;
            if (grayStamina <  0.0f) { grayStamina = 0.0f; }
            inDangerZone = (stamina <= DANGER_ZONE_PERCENT);
        }
    }

    public void OnPlayerStruck(DamageInfo info)
    {
        if (info.damageType == DamageType.Ball)
        {
            if (inDangerZone)
            {
                healthComponent.OnEntityDeath(info, healthComponent);
                return;
            }
            maxStamina -= BALL_MAX_STAMINA_DAMAGE;
            if (stamina > maxStamina) { stamina = maxStamina; }
        }
        DamageStamina(info.damage, info.dealsGrayStaminaDamage);
       
    }

    public void OnBallDeflected(bool partialDeflect)
    {
        if (!partialDeflect) { return; }
        DamageStamina(PARTIAL_DEFLECT_STAMINA_DAMAGE, true);

    }

    public void DamageStamina(int amount, bool useGrayStamina)
    {
        if (useGrayStamina) grayStamina += amount;

        stamina -= amount;
        if (useGrayStamina)
        {
            grayStamina += amount;
            if (stamina + grayStamina > maxStamina) { grayStamina = maxStamina - stamina; }
        }
        StartCoroutine(DelayStaminaRecoveryPostHit());
    }

    IEnumerator DelayStaminaRecoveryPostHit()
    {
        regenStamina = false;
        yield return new WaitForSeconds(STAMINA_USAGE_REGEN_DELAY);
        regenStamina = true;
    }

    public void OnPlayerDeflect()
    {
        stamina += grayStamina;
        grayStamina = 0.0f;
        if (stamina > maxStamina) { stamina = maxStamina; }
    }

    public float GetStamina()
    {
        return stamina;
    }

    public float GetGrayStamina()
    {
        return grayStamina;
    }
}
