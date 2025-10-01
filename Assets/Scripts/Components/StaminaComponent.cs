using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StaminaComponent : MonoBehaviour
{

    public UnityEvent regainedGrayStamina = new();

    [SerializeField] HealthComponent healthComponent;
    [SerializeField] DeflectManager deflectManager;

    const float STAMINA_REGEN_RATE = 7.5f;
    const float STAMINA_USAGE_REGEN_DELAY = 1.5f;
    const int BALL_MAX_STAMINA_DAMAGE = 25;
    const int DANGER_ZONE_PERCENT = 25;
    const int PARTIAL_DEFLECT_STAMINA_DAMAGE = 20;
    const float SUDDEN_DEATH_STAMINA_DRAIN_DELAY = 3.333f;

    float stamina = 100f;
    float maxStamina = 100f;
    float grayStamina = 0.0f;

    bool inDangerZone = false;
    bool inSuddenDeath = false;



    float delayTracker = 0.0f;
    float suddenDeathTracker = 0.0f;

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
        if (delayTracker <= 0.001f)
        {
            float staToAdd = STAMINA_REGEN_RATE * Time.deltaTime;
            stamina += staToAdd;
            if (stamina > maxStamina) { stamina = maxStamina; }
            grayStamina -= staToAdd;
            if (grayStamina <  0.0f) { grayStamina = 0.0f; }
        }
        inDangerZone = (stamina <= DANGER_ZONE_PERCENT);
        delayTracker -= Time.deltaTime;
        if (delayTracker <= 0.0f) { delayTracker = 0.0f; }

        if (inSuddenDeath)
        {
            SuddenDeathLogic();
        }
    }

    void SuddenDeathLogic()
    {
        suddenDeathTracker -= Time.deltaTime;
        if (suddenDeathTracker <= 0.0f)
        {
            suddenDeathTracker = SUDDEN_DEATH_STAMINA_DRAIN_DELAY;
            if (maxStamina > 1)
            {
                maxStamina -= 1;
            }
            
        }
    }

    public void OnPlayerStruck(DamageInfo info)
    {
        if (info.damageType == DamageType.Ball)
        {
            if (inDangerZone)
            {
                Debug.Log("Killing player");
                healthComponent.OnEntityDeath(info, healthComponent);
                return;
            }
            maxStamina -= BALL_MAX_STAMINA_DAMAGE;
        }
        DamageStamina(info.damage, info.dealsGrayStaminaDamage);
       
    }

    public void OnBallDeflected(bool partialDeflect)
    {
        if (!partialDeflect)
        {
            if (grayStamina > 0.0f) { regainedGrayStamina.Invoke(); }
            stamina += grayStamina;
            grayStamina = 0.0f;
            stamina = Mathf.Clamp(stamina, 1, maxStamina);
        }
        else
        {
            DamageStamina(PARTIAL_DEFLECT_STAMINA_DAMAGE, true);
        }

    }

    public void DamageStamina(int amount, bool useGrayStamina)
    {
        stamina = Mathf.Clamp(stamina, 1, maxStamina);
        stamina -= amount;
        if (useGrayStamina)
        {
            grayStamina += amount;
            if (stamina + grayStamina > maxStamina) { grayStamina = maxStamina - stamina; }
        }
        stamina = Mathf.Clamp(stamina, 1, maxStamina);
        DelayStaminaRecoveryPostHit();
     
    }

    void DelayStaminaRecoveryPostHit()
    {
        delayTracker = STAMINA_USAGE_REGEN_DELAY;
    }
    public float GetStamina()
    {
        return stamina;
    }

    public float GetGrayStamina()
    {
        return grayStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public bool InDangerZone()
    {
        return inDangerZone;
    }

    public void EnterSuddenDeath()
    {
        suddenDeathTracker = SUDDEN_DEATH_STAMINA_DRAIN_DELAY;
        inSuddenDeath = true;
    }
}
